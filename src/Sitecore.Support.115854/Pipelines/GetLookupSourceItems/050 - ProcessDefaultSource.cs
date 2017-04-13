namespace Sitecore.Support.Pipelines.GetLookupSourceItems
{
  using System;
  using Sitecore.Collections;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Support.Extensions.SitecoreQueryExtensions;
  using Sitecore.Pipelines.GetLookupSourceItems;
  using Sitecore.Text;
  using Sitecore.Web.UI.HtmlControls.Data;

  /// <summary>
  ///   Processes common query sources without well-known prefixes.
  /// </summary>
  public class ProcessDefaultSource
  {
    #region Public methods

    /// <summary>
    ///   Processes common query sources without well-known prefixes.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public void Process([NotNull] GetLookupSourceItemsArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      if (string.IsNullOrEmpty(args.Source))
      {
        return;
      }

      string source = args.Source;
      Item rootItem = args.Item;

      if (this.IsComplex(source))
      {
        source = this.BuildComplexSource(source, ref rootItem);
      }

      string[] paths = source.Split('|');

      foreach (string path in paths)
      {
        if (ID.IsID(path))
        {
          Item item = rootItem.Database.GetItem(path);

          if (item == null)
          {
            continue;
          }

          ChildList children = item.Children;

          if (children.Count > 0)
          {
            args.Result.AddRange(children.ToArray());
          }
        }
        else
        {
          string queryPath = this.CreateQuery(path);
        
          queryPath += "/*";

          Item[] r = queryPath.StartsWithFastQueryPrefix() ? rootItem.Database.SelectItems(queryPath) : rootItem.Axes.SelectItems(queryPath);

          if (r != null && r.Length > 0)
          {
            args.Result.AddRange(r);
          }
        }
      }
    }

    #endregion

    #region Protected methods

    /// <summary>
    ///   Determines whether the <paramref name="source" /> is a complex source.
    ///   <para>Source considered as complex in case it has 'datasource=', or 'databasename=' markers specified.</para>
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>
    ///   <c>true</c> if the source is complex; otherwise, <c>false</c>.
    /// </returns>
    /// <contract>
    ///   <requires name="source" condition="not null" />
    /// </contract>
    protected virtual bool IsComplex(string source)
    {
      Assert.ArgumentNotNull(source, "source");
      return LookupSources.IsComplex(source);
    }

    /// <summary>
    ///   Builds the complex source from provided text in <see cref="UrlString"/> format.
    ///   <para>Source extracted from <see cref="UrlString" /> 'datasource' parameter.</para>
    ///   <para>Returns <see cref="string.Empty" /> in case no data found.</para>
    /// </summary>
    /// <param name="source">The source in <see cref="UrlString" /> format.</param>
    /// <param name="rootItem">The root item of the database defined in <paramref name="source" />.</param>
    /// <returns>
    ///   <see cref="string.Empty" /> if source does not have 'datasource' parameter set; 'datasource' parameter
    ///   otherwise.
    /// </returns>
    [NotNull]
    protected virtual string BuildComplexSource([NotNull] string source, ref Item rootItem)
    {
      Debug.ArgumentNotNull(source, "source");

      var parameters = new UrlString(source);

      string databaseName = parameters["databasename"];

      if (!string.IsNullOrEmpty(databaseName))
      {
        Database database = Factory.GetDatabase(databaseName);
        Assert.IsNotNull(database, databaseName);

        rootItem = database.GetRootItem(rootItem.Language);
        Assert.IsNotNull(rootItem, Texts.ROOT_ITEM_IN_DATABASE_0_NOT_FOUND, databaseName);
      }

      source = parameters["datasource"] ?? string.Empty;
      return source;
    }

    /// <summary>
    ///   Creates and escapes Sitecore query from provided path.
    /// </summary>
    /// <param name="queryPath">The query path.</param>
    /// <returns>Constructed, and escaped Sitecore Query.</returns>    
    [NotNull]
    protected virtual string CreateQuery([NotNull] string queryPath)
    {
      Debug.ArgumentNotNull(queryPath, "queryPath");

      if (queryPath.StartsWith("/", StringComparison.InvariantCulture) && !queryPath.StartsWith("/sitecore", StringComparison.OrdinalIgnoreCase) && !queryPath.StartsWith("/" + ItemIDs.RootID, StringComparison.OrdinalIgnoreCase))
      {
        queryPath = "/sitecore" + queryPath;
      }      

      queryPath = queryPath.EscapeQueryPath();

      return queryPath;
    }

    #endregion
  }
}