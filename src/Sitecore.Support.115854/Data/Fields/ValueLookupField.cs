namespace Sitecore.Support.Data.Fields
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Fields;
  using Sitecore.Diagnostics;
  using Sitecore.Links;
  using Sitecore.Text;
  using Sitecore.Web.UI.HtmlControls.Data;

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
      Assert.ArgumentNotNull(result, nameof(result));
      var value = Value;
      if (string.IsNullOrEmpty(value))
      {
        return;
      }

      if (InnerField.Source.Length == 0)
      {
        return;
      }
      var item = InnerField.Item;
      var datasource = InnerField.Source;
      if (LookupSources.IsComplex(InnerField.Source))
      {
        var source = new UrlString(InnerField.Source);
        var databaseName = source["databasename"];
        if (!string.IsNullOrEmpty(databaseName))
        {
          var database = Factory.GetDatabase(databaseName);
          Assert.IsNotNull(database, $"{nameof(database)}: {databaseName}");

          item = database.GetRootItem(base.InnerField.Item.Language);
          Assert.IsNotNull(item, $"Root item in database \"{databaseName}\" not found");
        }

        datasource = source["datasource"];
      }

      var sources = datasource.Split('|');

      //string[] array2 = array;
      for (var i = 0; i < sources.Length; i++)
      {
        var source = sources[i];
        if (ID.IsID(source))
        {
          var item2 = item.Database.GetItem(source);
          if (item2 == null)
          {
            continue;
          }

          var targetItem = item.Database.GetItem(item2.Paths.FullPath + '/' + value);
          if (targetItem == null)
          {
            continue;
          }

          result.AddValidLink(targetItem, value);

          return;
          //ChildList children = item2.Children;
          //if (children.Count > 0)
          //{
          //    args.Result.AddRange(children.ToArray());
          //}
        }
        else
        {
          if (source.StartsWith("/", System.StringComparison.InvariantCulture) && !source.StartsWith("/sitecore", System.StringComparison.OrdinalIgnoreCase) && !source.StartsWith("/" + ItemIDs.RootID, System.StringComparison.OrdinalIgnoreCase))
          {
            source = "/sitecore" + source;
          }

          var targetItem = item.Database.GetItem(source + '/' + value);
          if (targetItem == null)
          {
            continue;
          }

          result.AddValidLink(targetItem, value);
          return;
        }
      }
          
      //Item[] items = LookupSources.GetItems(base.InnerField.Item, base.InnerField.Source);
      //Item[] array = items;
      //for (int i = 0; i < array.Length; i++)
      //{
      //    Item item = array[i];
      //    if (value == item.Name)
      //    {
      //        result.AddValidLink(item, value);
      //        return;
      //    }
      //}

      result.AddBrokenLink(value);
    }
  }
}
