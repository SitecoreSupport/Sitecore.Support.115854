//--------------------------------------------------------------------------------------------------------------------
// <copyright company="Sitecore Corporation" file="GetLookupSourceTargetItemArgs.cs">
//   Copyright (C) 2017 by Sitecore Corporation
// </copyright>
// <summary>
//   Argument class for 'getLookupSourceTargetItem' pipeline that resolves target item by provided parameters.
// </summary>
// -------------------------------------------------------------------------------------------------------------------- 
namespace Sitecore.Support.Pipelines.GetLookupSourceTargetItem
{
  using System.Diagnostics;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines;
  using Sitecore.Pipelines.GetLookupSourceItems;

  /// <summary>
  ///   Argument class for 'getLookupSourceTargetItem' pipeline that resolves target item by provided parameters.
  ///   <para>Carries <see cref="Source" />, <see cref="Value" />, and <see cref="Item" /> itself.</para>
  ///   <para>Value is considered as valid if <see cref="TargetItem" /> can be resolved using provided parameters.</para>
  /// </summary>
  [DebuggerDisplay("Source={_source} Value={_value} TargetItemResolved={TargetItemResolved}")]
  public class GetLookupSourceTargetItemArgs : PipelineArgs
  {
    #region Fields

    private Item _item;

    private string _source;

    private string _value;

    #endregion

    #region Public properties

    /// <summary>
    ///   Gets or sets the item.
    /// </summary>
    /// <value>
    ///   The item.
    /// </value>
    [NotNull]
    public virtual Item Item
    {
      get
      {
        return _item;
      }

      set
      {
        Assert.ArgumentNotNull(value, nameof(value));
        _item = value;
      }
    }

    /// <summary>
    ///   Gets or sets the source.
    /// </summary>
    /// <value>
    ///   The source.
    /// </value>
    [NotNull]
    public virtual string Source
    {
      get
      {
        return _source;
      }

      set
      {
        Assert.ArgumentNotNull(value, nameof(value));
        _source = value;
      }
    }

    /// <summary>
    ///   Gets or sets the target item.
    /// </summary>
    /// <value>
    ///   The target item.
    /// </value>
    [CanBeNull]
    public virtual Item TargetItem { get; set; }


    /// <summary>
    /// Gets a value indicating whether a <see cref="TargetItem"/> is set.
    /// </summary>
    /// <value>
    ///   <c>true</c> if target item is resolved; otherwise, <c>false</c>.
    /// </value>
    public bool TargetItemResolved => TargetItem != null;

    /// <summary>
    ///   Gets or sets the value.
    /// </summary>
    /// <value>
    ///   The value.
    /// </value>
    [NotNull]
    public virtual string Value
    {
      get
      {
        return _value;
      }

      set
      {
        Assert.ArgumentNotNull(value, nameof(value));
        _value = value;
      }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the target item, and aborts pipeline.
    /// </summary>
    /// <param name="targetItem">The target item.</param>
    public void SetResultAndAbort([CanBeNull] Item targetItem)
    {
      TargetItem = targetItem;
      AbortPipeline();
    }

    /// <summary>
    /// Produces a new instance of <see cref="GetLookupSourceItemsArgs"/> from current instance.
    /// </summary>
    /// <returns>An initiated instance of the args from current instance.</returns>
    [NotNull]
    public virtual GetLookupSourceItemsArgs ToLookupSourceItemArgs()
    {
      var args = new GetLookupSourceItemsArgs
      {
        Item = Item,
        Source = Source
      };

      return args;
    }
    #endregion
  }
}