namespace Sitecore.Support.Data.Fields
{
  using Sitecore.Data.Fields;
  using Sitecore.Diagnostics;
  using Sitecore.Links;
  using Sitecore.Support.Pipelines.GetLookupSourceTargetItem;

  public class ValueLookupField : Sitecore.Data.Fields.ValueLookupField
  {
    [UsedImplicitly]
    public ValueLookupField(Field innerField) : base(innerField)
    {
    }

    /// <summary>
    /// Validates the links.
    /// </summary>
    /// <param name="result">The result.</param>
    public override void ValidateLinks(LinksValidationResult result)
    {
      Assert.ArgumentNotNull(result, "result");

      string fieldValue = this.Value;
      string fieldSource = this.InnerField.Source;

      if (string.IsNullOrEmpty(fieldValue) || string.IsNullOrEmpty(fieldSource))
      {
        return;
      }

      var targetItem = GetLookupSourceTargetItemPipeline.GetTargetItem(this.InnerField.Item, fieldSource, fieldValue);

      if (targetItem != null)
      {
        result.AddValidLink(targetItem, targetPath: fieldValue);        
      }
      else
      {
        result.AddBrokenLink(targetPath: fieldValue);
      }      
    }
  }
}