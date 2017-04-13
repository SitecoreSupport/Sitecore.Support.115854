//--------------------------------------------------------------------------------------------------------------------
// <copyright company="Sitecore Corporation" file="GetLookupSourceTargetItemPipeline.cs">
//   Copyright (C) 2017 by Sitecore Corporation
// </copyright>
// <summary>
//   Pipeline resolves an item by provided source (f.e. Sitecore Query, plain item path, ID), and extra value appended (f.e. item name).
// </summary>
// -------------------------------------------------------------------------------------------------------------------- 
namespace Sitecore.Support.Pipelines.GetLookupSourceTargetItem
{
  using System;
  using Sitecore.Support.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Exceptions;
  using Sitecore.Pipelines;
  using Sitecore.Support.Pipelines.GetLookupSourceTargetItem;

  /// <summary>
  ///   Resolves a single target item using provided source, field value, and content item.  
  /// <para>The result is available in <see cref="GetLookupSourceTargetItemArgs.TargetItem"/> property.</para>
  /// <para>Used by <see cref="ValueLookupField"/> to validate links.</para>
  /// </summary>
  public static class GetLookupSourceTargetItemPipeline
  {
    #region Fields

    /// <summary>
    ///   The 'getLookupSourceTargetItem' pipeline name.
    /// </summary>
    public static readonly string PipelineName = "getLookupSourceTargetItem";

    /// <summary>
    /// The get lookup source target item pipeline.
    /// </summary>
    public static readonly CorePipeline LookupSourceTargetItemPipeline;

    #endregion

    /// <summary>
    /// Initializes static members of the <see cref="GetLookupSourceTargetItemPipeline"/> class.
    /// </summary>
    static GetLookupSourceTargetItemPipeline()
    {
      LookupSourceTargetItemPipeline = CorePipelineFactory.GetPipeline(PipelineName, string.Empty);      
    }

    #region Public methods

    /// <summary>
    ///   Resolves Target item via 'getLookupSourceTargetItem' pipeline execution.
    ///   <para>Makes an attempt to map provided source input and value to a Sitecore item.</para>
    /// </summary>
    /// <param name="item">The item to have validation for.</param>
    /// <param name="source">The content source.</param>
    /// <param name="fieldValue">The field value.</param>
    /// <returns><c>null</c> when could not resolve item by given parameters; resolved target item otherwise.</returns>
    [CanBeNull]
    public static Item GetTargetItem([NotNull] Item item, [NotNull] string source, [NotNull] string fieldValue)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(source, "source");
      Assert.ArgumentNotNull(fieldValue, "fieldValue");

      var args = new GetLookupSourceTargetItemArgs
      {
        Item = item,
        Source = source,
        Value = fieldValue
      };

      GetLookupSourceTargetItemPipeline.Run(args);

      return args.TargetItem;
    }

    /// <summary>
    ///   Runs the 'validateLookupSourceLink' pipeline with arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static void Run([NotNull] GetLookupSourceTargetItemArgs args)
    {
      Assert.ArgumentNotNull(args, "args");

      try
      {
        using (new LongRunningOperationWatcher(1000, $"{PipelineName} pipeline[item={args.Item.Uri}, source={args.Source}]"))
        {
          LookupSourceTargetItemPipeline?.Run(args);
        }
      }
      catch (Exception e)
      {
        throw new LookupSourceException(args.Source, e);
      }
    }

    #endregion
  }
}