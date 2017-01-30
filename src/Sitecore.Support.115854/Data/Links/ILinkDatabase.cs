namespace Sitecore.Support.Data.Links
{
  using System;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Links;

  public interface ILinkDatabase
  {
    void Compact(Database database);

    ItemLink[] GetBrokenLinks(Database database);

    int GetReferenceCount(Item item);

    ItemLink[] GetReferences(Item item);

    int GetReferrerCount(Item item);
                  
    ItemLink[] GetReferrers(Item item);

    [Obsolete]
    ItemLink[] GetReferrers(Item item, bool deep);

    void RemoveReferences(Item item);

    void UpdateLinks(Item item, ItemLink[] links);
  }
}