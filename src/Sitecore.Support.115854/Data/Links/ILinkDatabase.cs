namespace Sitecore.Support.Data.Links
{
  using System;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Links;

  public interface ILinkDatabase
  {
    void AddLink(Item item, ItemLink link);

    void AddModifiedItemLinks(Item item, ItemLink[] itemLinkReferences, ItemLink[] contextitemLinks);

    void Compact(Database database);

    ItemLink[] GetBrokenLinks(Database database);

    ItemLink[] GetItemVersionReferrers(Item version);

    int GetReferenceCount(Item item);

    ItemLink[] GetReferences(Item item);

    int GetReferrerCount(Item item);

    ItemLink[] GetReferrers(Item item);

    ItemLink[] GetReferrers(Item item, ID sourceFieldId);
               
    [Obsolete]
    ItemLink[] GetReferrers(Item item, bool deep);

    bool HasExternalReferrers(Item item, bool deep);

    bool LinkExistsInTargetItemLinks(ItemLink[] targetItemLinks, ItemLink itemLink);

    void Rebuild(Database database);

    void RemoveItemVersionLink(ItemLink itemLink);

    void RemoveReferences(Item item);

    void UpdateItemVersionLinks(Item item, ItemLink[] links);

    void UpdateItemVersionReferences(Item item);

    void UpdateLinks(Item item, ItemLink[] links);

    void UpdateReferences(Item item);
  }
}