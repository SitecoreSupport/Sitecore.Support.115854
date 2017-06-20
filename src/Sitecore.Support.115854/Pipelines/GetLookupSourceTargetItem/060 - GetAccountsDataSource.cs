namespace Sitecore.Support.Pipelines.GetLookupSourceTargetItem
{
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Social.Infrastructure;

  public class GetAccountsDataSource : Social.Client.Pipelines.GetLookupSourceItems.GetAccountsDataSource
  {
    /// <summary>    
    /// <para>Processes social-related links using <see cref="GetAccountsDataSource"/> code.</para>    
    /// </summary>
    /// <param name="args">The arguments.</param>
    public virtual void Execute([NotNull] GetLookupSourceTargetItemArgs args)
    {
      if (!ExecutingContext.Current.IoCState.IoCLoaded())
      {
        return;
      }

      Assert.ArgumentNotNull(args, nameof(args));

      var lookupArgs = args.ToLookupSourceItemArgs();

      // TODO: Think about better approach instead of loading all items.
      Process(lookupArgs);

      foreach (Item lookupTargetItem in lookupArgs.Result)
      {
        if (Matches(lookupTargetItem, args))
        {
          args.SetResultAndAbort(lookupTargetItem);
        }
      }      
    }

    /// <summary>
    /// Checks if the specified lookup target item matches value from args.
    /// </summary>
    /// <param name="lookupTargetItem">The lookup target item.</param>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    protected virtual bool Matches([NotNull] Item lookupTargetItem, [NotNull] GetLookupSourceTargetItemArgs args) => lookupTargetItem.Name == args.Value; // TODO: test
  }
}