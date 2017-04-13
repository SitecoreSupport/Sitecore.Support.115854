//--------------------------------------------------------------------------------------------------------------------
// <copyright company="Sitecore Corporation" file="010 - ProcessQuerySource.cs">
//   Copyright (C) 2017 by Sitecore Corporation
// </copyright>
// <summary>
//   Handles target item resolution when query source is Sitecore Query (starts with 'query:' prefix).
// </summary>
// -------------------------------------------------------------------------------------------------------------------- 
namespace Sitecore.Support.Pipelines.GetLookupSourceTargetItem
{
  using Sitecore.Diagnostics;  

  /// <summary>
  ///   Processes query sources that start with 'query:' prefix.
  /// </summary>
  public class ProcessQuerySource : GetLookupSourceItems.ProcessQuerySource
  {
    #region Public methods

    /// <summary>
    ///   Validates the lookup source link for Sitecore query format.
    /// <para>Appends field value to query, and computes it. Sets computed item as pipeline execution result, and aborts it.</para>
    /// </summary>
    /// <param name="args">The arguments.</param>
    public virtual void Execute([NotNull] GetLookupSourceTargetItemArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      
      if (string.IsNullOrEmpty(args.Source) || string.IsNullOrEmpty(args.Value))
      {
        return;
      }

      bool isFastQuery;
      string sitecoreQuery;

      if (!this.IsSitecoreQuery(args.Source, out sitecoreQuery, out isFastQuery))
      {
        return;
      }

      sitecoreQuery = string.Concat(sitecoreQuery, '/', args.Value);

      var targetItem = isFastQuery ? args.Item.Database.SelectSingleItem(sitecoreQuery) : args.Item.Axes.SelectSingleItem(sitecoreQuery);
      
      args.SetResultAndAbort(targetItem);
    }

    #endregion
  }
}