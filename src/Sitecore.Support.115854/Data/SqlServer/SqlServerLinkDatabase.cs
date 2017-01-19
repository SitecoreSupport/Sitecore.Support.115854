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
  using static Sitecore.Diagnostics.PerformanceCounters.LinkCounters;

  public class ParallelSqlServerLinkDatabase : SqlServerLinkDatabase
  {
    [UsedImplicitly]
    public ParallelSqlServerLinkDatabase(string connectionString) : base(connectionString)
    {
    }

    private static readonly bool LinkDatabaseParallelRebuildEnabled;

    private static readonly TaskScheduler taskScheduler;

    static ParallelSqlServerLinkDatabase()
    {
      LinkDatabaseParallelRebuildEnabled = Settings.GetBoolSetting("LinkDatabase.ParallelRebuild.Enabled", false);
      if (LinkDatabaseParallelRebuildEnabled)
      {
        taskScheduler = new LimitedConcurrencyLevelTaskScheduler(Settings.GetIntSetting("LinkDatabase.ParallelRebuild.MaxThreadLimit", Environment.ProcessorCount));
      }
    }

    public override void Rebuild(Database database)
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
        RebuildItemInParallel2(new Tuple<Item, RebuildState>(rootItem, state));
        while (state.PendingCrawlCount > 0L)
        {
          Thread.Sleep(50);
        }
      }

      Compact(database);
    }

    public override void Compact(Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      var batchSize = Settings.LinkDatabase.MaximumBatchSize;

      var lastProcessed = BatchCompact(database, batchSize, ID.Null);

      while (lastProcessed != ID.Null)
      {
        lastProcessed = BatchCompact(database, batchSize, lastProcessed);
      }
    }

    private ID BatchCompact([NotNull]Database database, int batchSize, ID lastProcessed)
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

      Guid linkGuid = Guid.Empty;

      var dataTable = new DataTable();

      using (var reader = DataApi.CreateReader(fetchSql, "database", database.Name, "lastProcessed", lastProcessed))
      {
        dataTable.Load(reader.InnerReader);
      }

      DataRead.IncrementBy(dataTable.Rows.Count);

      foreach (DataRow dataRow in dataTable.Rows)
      {
        //for (int i = 0; i < dataTable.Columns.Count; i++)
        //{
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
        //}
      }

      return new ID(linkGuid);
    }

    private void RebuildItemInParallel2(object o)
    {
      var tuple = (Tuple<Item, RebuildState>)o;
      var item = tuple.Item1;
      var state = tuple.Item2;
      try
      {
        using (new SecurityDisabler())
        {
          this.UpdateReferences(item);
          var children = item.GetChildren(ChildListOptions.None).ToArray();
          foreach (var child in children)
          {
            Interlocked.Increment(ref state.PendingCrawlCount);

            var task = new Task(RebuildItemInParallel2, new Tuple<Item, RebuildState>(child, state), CancellationToken.None, TaskCreationOptions.DenyChildAttach);
            task.Start(taskScheduler);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("SUPPORT: error during Link database rebuild. Item: " + item.Uri, ex, this);
      }
      finally
      {
        Interlocked.Decrement(ref state.PendingCrawlCount);
      }
    }

    private class RebuildState
    {                                              
      public long PendingCrawlCount = 0;                                                          
    }
  }
}