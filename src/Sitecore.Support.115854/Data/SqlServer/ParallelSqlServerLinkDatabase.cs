namespace Sitecore.Support.Data.SqlServer
{
  using System;
  using System.Data;
  using System.Threading;
  using System.Threading.Tasks;
  using Sitecore.Collections;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.SqlServer;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.SecurityModel;
  using Sitecore.Threading;

  /// <summary>
  /// Enables rebuild of links in multiple threads with predefined number threads to be used.
  /// <para>Each thread acts as producer, and consumer now.</para>
  /// </summary>
  /// <seealso cref="Sitecore.Data.SqlServer.SqlServerLinkDatabase"/>
  public class ParallelSqlServerLinkDatabase : SqlServerLinkDatabase
  {
    #region Fields

    private static readonly bool LinkDatabaseParallelRebuildEnabled;

    private static readonly TaskScheduler TaskScheduler;

    #endregion

    #region constructors

    /// <summary>
    /// Initializes static members of the <see cref="ParallelSqlServerLinkDatabase"/> class.
    /// <para>Reads <see cref="Settings"/> related to link rebuild.</para>
    /// </summary>
    static ParallelSqlServerLinkDatabase()
    {
      LinkDatabaseParallelRebuildEnabled = Settings.GetBoolSetting("LinkDatabase.ParallelRebuild.Enabled", defaultValue: false);
      if (LinkDatabaseParallelRebuildEnabled)
      {
        TaskScheduler = new LimitedConcurrencyLevelTaskScheduler(Settings.GetIntSetting("LinkDatabase.ParallelRebuild.MaxThreadLimit", defaultValue: Environment.ProcessorCount));
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelSqlServerLinkDatabase"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string leading to database.</param>
    [UsedImplicitly]
    public ParallelSqlServerLinkDatabase(string connectionString) : base(connectionString)
    {
    }

    #endregion

    #region Public methods

    public override void Compact([NotNull] Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      var batchSize = Settings.LinkDatabase.MaximumBatchSize;

      var lastProcessed = ID.Null;

      do
      {
        lastProcessed = BatchCompact(database, batchSize, lastProcessed);
      }
      while (lastProcessed != ID.Null);
    }

    /// <summary>
    /// Rebuilds links for specified database.
    /// <para>Recursively travels through Content tree, and rebuilds links for each item.</para>
    /// <para>Uses <see cref="TaskScheduler"/> to limit number of threads during rebuild operation.</para>
    /// </summary>
    /// <param name="database">The database to rebuild links for.</param>
    public override void Rebuild([NotNull] Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      if (!LinkDatabaseParallelRebuildEnabled)
      {
        base.Rebuild(database);

        return;
      }

      using (new SecurityDisabler())
      {
        var rootItem = database.GetRootItem(Context.Language);
        Assert.IsNotNull(rootItem, $"No root item in database: {database.Name}");

        var state = new RebuildState();

        Interlocked.Increment(ref state.PendingCrawlCount);
        RebuildLinksRecursively(new Tuple<Item, RebuildState>(rootItem, state));

        // wait till all are processed
        while (state.PendingCrawlCount > 0L)
        {
          Thread.Sleep(millisecondsTimeout: 50);
        }
      }

      Compact(database);
    }

    #endregion

    #region Non-public members

    protected virtual ID BatchCompact([NotNull] Database database, int batchSize, ID lastProcessed)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      // remove referrers from items that no longer exist
      var fetchSql = @"
SELECT TOP " + batchSize + @"
	{0}ID{1},
	{0}SourceItemID{1},
	{0}SourceLanguage{1},
	{0}SourceVersion{1}
FROM
	{0}Links{1} WITH (NOLOCK)
WHERE
	{0}SourceDatabase{1} = {2}database{3} " +
                     (lastProcessed == ID.Null ? string.Empty : @" AND {0}ID{1} > {2}lastProcessed{3} ") + @"
ORDER BY
	{0}ID{1}";

      var deleteSql = @"
DELETE FROM {0}Links{1} 
WHERE {0}ID{1} = {2}id{3}";

      var linkGuid = Guid.Empty;

      var dataTable = new DataTable();

      using (var reader = DataApi.CreateReader(fetchSql, "database", database.Name, "lastProcessed", lastProcessed))
      {
        dataTable.Load(reader.InnerReader);
      }    

      foreach (DataRow dataRow in dataTable.Rows)
      {
        linkGuid = new Guid(dataRow[0].ToString());
        var sourceItemGuid = new Guid(dataRow[1].ToString());
        var sourceItemLanguage = Language.Parse(dataRow[2].ToString());
        var sourceItemVersion = Sitecore.Data.Version.Parse(int.Parse(dataRow[3].ToString()));
        var sourceItemId = ID.Parse(sourceItemGuid);

        if (ItemExists(sourceItemId, null, sourceItemLanguage, sourceItemVersion, database))
        {
          continue;
        }

        DataApi.Execute(deleteSql, "id", linkGuid);
      }

      return new ID(linkGuid);
    }

    protected virtual void RebuildLinksRecursively(object taskState)
    {
      var tuple = (Tuple<Item, RebuildState>)taskState;
      var item = tuple.Item1;
      var state = tuple.Item2;
      try
      {
        using (new SecurityDisabler())
        {
          TryUpdateItemLinks(item);

          var children = item.GetChildren(ChildListOptions.AllowReuse | ChildListOptions.IgnoreSecurity | ChildListOptions.SkipSorting);

          foreach (Item child in children)
          {
            Interlocked.Increment(ref state.PendingCrawlCount);

            var task = new Task(
              action: RebuildLinksRecursively,
              state: new Tuple<Item, RebuildState>(child, state),
              cancellationToken: CancellationToken.None,
              creationOptions: TaskCreationOptions.DenyChildAttach);

            task.Start(TaskScheduler);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error($"An error occurred during Link database rebuild. Item: { item.Uri }", ex, this);
      }
      finally
      {
        Interlocked.Decrement(ref state.PendingCrawlCount);
      }
    }

    /// <summary>
    /// Tries to update item links,and logs long-running link rebuilds.
    /// </summary>
    /// <param name="item">The item to have links updated.</param>
    /// <returns><c>false</c> if exception took place;<c>true</c> otherwise.</returns>
    protected virtual bool TryUpdateItemLinks([NotNull] Item item)
    {
      Debug.ArgumentNotNull(item, nameof(item));
      try
      {
        var threshold = 2 * 1000; // 2 seconds
        using (new LongRunningOperationWatcher(threshold, "Updating links for {0} item takes more than 2 sec.", item.Uri.ToString()))
        {
          UpdateReferences(item);
        }

        return true;
      }
      catch (Exception exception)
      {
        Log.Error($"Failed to update {item.Uri} links.", exception, this);
        return false;
      }
    }

    #endregion

    #region Nested classes
    /// <summary>
    /// Carries the count of pending items to be processed.
    /// </summary>
    private class RebuildState
    {
      public long PendingCrawlCount;
    }
    #endregion
  }
}