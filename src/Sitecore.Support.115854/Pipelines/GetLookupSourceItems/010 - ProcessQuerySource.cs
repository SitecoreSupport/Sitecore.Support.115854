namespace Sitecore.Support.Pipelines.GetLookupSourceItems
{
  using Sitecore.Data.Items;
  using Sitecore.Data.Query;
  using Sitecore.Diagnostics;
  using Sitecore.Support.Extensions.SitecoreQueryExtensions;
  using Sitecore.Pipelines.GetLookupSourceItems;

  /// <summary>
  ///   Processes query sources that start with 'query:' prefix
  /// </summary>
  public class ProcessQuerySource
  {
    #region Public methods

    /// <summary>
    ///   Runs the processor.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public virtual void Process([NotNull] GetLookupSourceItemsArgs args)
    {
      Assert.ArgumentNotNull(args, nameof(args));

      bool isFastQuery;

      string sitecoreQuery;

      if (!IsSitecoreQuery(args.Source, out sitecoreQuery, out isFastQuery))
      {
        return;
      }

      Item[] result = isFastQuery ? args.Item.Database.SelectItems(sitecoreQuery) : args.Item.Axes.SelectItems(sitecoreQuery);

      if (result != null && result.Length > 0)
      {
        args.Result.AddRange(result);
      }

      args.AbortPipeline();
    }
    #endregion
    #region Protected methods
    /// <summary>
    /// Extracts the Sitecore query body from provided source, and invokes <see cref="QueryParser"/> to parse (validate).
    /// <para>Returns <c>false</c> if <paramref name="text"/> is <c>null</c>, or not Sitecore query; <paramref name="queryBody"/> set otherwise.</para>
    /// </summary>
    /// <param name="text">The query text source.</param>
    /// <param name="queryBody">The extracted query body.</param>
    /// <param name="isFastQuery">if set to <c>true</c> the <paramref name="text" /> carries Sitecore Fast query.</param>
    /// <returns>
    /// <para>Returns <c>false</c> if <paramref name="text"/> is <c>null</c>, or not Sitecore query provided in input;<c>true</c> otherwise.</para>  
    /// </returns>
    protected virtual bool IsSitecoreQuery([NotNull] string text, [NotNull] out string queryBody, out bool isFastQuery)
    {
      Debug.ArgumentNotNull(text, nameof(text));

      isFastQuery = false;

      if (!text.TrySubstringSitecoreQueryBody(out queryBody))
      {
        return false;
      }

      isFastQuery = queryBody.StartsWithFastQueryPrefix();

      if (!isFastQuery)
      {
        QueryParser.Parse(queryBody);
      }

      return true;
    }
    #endregion
  }
}