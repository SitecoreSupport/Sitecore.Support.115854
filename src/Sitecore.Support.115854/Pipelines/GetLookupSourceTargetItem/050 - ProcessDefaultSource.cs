//--------------------------------------------------------------------------------------------------------------------
// <copyright company="Sitecore Corporation" file="050 - ProcessDefaultSource.cs">
//   Copyright (C) 2017 by Sitecore Corporation
// </copyright>
// <summary>
//   Handles target item resolution when query source is Sitecore path, itemID, or carries datasource inside.
// </summary>
// -------------------------------------------------------------------------------------------------------------------- 
namespace Sitecore.Support.Pipelines.GetLookupSourceTargetItem
{
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Support.Extensions.SitecoreQueryExtensions;
  using Sitecore.Support.Pipelines.GetLookupSourceTargetItem;
  using Sitecore.Web.UI.HtmlControls.Data;

  /// <summary>
  ///   Processes common query sources (f.e. Sitecore path, item ID) without well-known prefixes.
  /// <para>Capable of extracting lookup datasource (see <see cref="LookupSources"/>).</para>
  /// <para>Appends <see cref="GetLookupSourceTargetItemArgs.Value"/> to <see cref="GetLookupSourceTargetItemArgs.Source"/> and locates item by resulting path.</para>
  /// </summary>
  public class ProcessDefaultSource : GetLookupSourceItems.ProcessDefaultSource
  {
    #region Public methods

    /// <summary>
    ///   Validates the lookup source link.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public virtual void Execute([NotNull] GetLookupSourceTargetItemArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      if (string.IsNullOrEmpty(args.Source) || string.IsNullOrEmpty(args.Value))
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
          Item sourceItem = rootItem.Database.GetItem(path);

          if (sourceItem == null)
          {
            continue;
          }
          var item = rootItem.Database.SelectSingleItem(sourceItem.Paths.FullPath + "/" + args.Value);

          if (item != null)
          {
            args.SetResultAndAbort(targetItem: item);
            return;           
          }       
        }
        else
        {
          var combinedPath = this.Combine(path, args.Value);
          string queryPath = this.CreateQuery(combinedPath);

          Item item = queryPath.StartsWithFastQueryPrefix() ? rootItem.Database.SelectSingleItem(queryPath) : rootItem.Axes.SelectSingleItem(queryPath);

          if (item == null)
          {
            continue;
          }

          args.SetResultAndAbort(targetItem: item);
          return;
        }
      }
    }

    /// <summary>
    /// Appends the value to Sitecore query.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="value">The value.</param>
    /// <returns>query with appended value.</returns>
    [NotNull]
    protected virtual string Combine([NotNull]string query, [CanBeNull]string value)
    {
      Debug.ArgumentNotNull(query, "query");

      return string.IsNullOrEmpty(value) ? query : string.Concat(query,'/', value);
    }

    #endregion
  }
}