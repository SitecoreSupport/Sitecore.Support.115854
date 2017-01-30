namespace Sitecore.Support.Data.Links
{
  using System;
  using System.Collections.Generic;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Globalization;
  using Sitecore.Links;

  public abstract class SwitchingLinkDatabase : LinkDatabase
  {
    [NotNull]
    private readonly IReadOnlyDictionary<string, ILinkDatabase> _Map;

    protected SwitchingLinkDatabase([NotNull] IReadOnlyDictionary<string, ILinkDatabase> map)
    {
      Assert.ArgumentNotNull(map, nameof(map));

      _Map = map;
    }

    protected override void AddLink([NotNull] Item item, [NotNull] ItemLink link)
    {
      Assert.ArgumentNotNull(item, nameof(item));
      Assert.ArgumentNotNull(link, nameof(link));

      _Map[item.Database.Name].AddLink(item, link);
    }

    protected override void AddModifiedItemLinks([NotNull] Item item, [NotNull] ItemLink[] itemLinkReferences, [NotNull] ItemLink[] contextitemLinks)
    {
      Assert.ArgumentNotNull(item, nameof(item));
      Assert.ArgumentNotNull(itemLinkReferences, nameof(itemLinkReferences));
      Assert.ArgumentNotNull(contextitemLinks, nameof(contextitemLinks));

      _Map[item.Database.Name].AddModifiedItemLinks(item, itemLinkReferences, contextitemLinks);
    }

    public override void Compact([NotNull] Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      _Map[database.Name].Compact(database);
    }

    public override ItemLink[] GetBrokenLinks([NotNull] Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      return _Map[database.Name].GetBrokenLinks(database);
    }

    public override ItemLink[] GetItemReferences(Item item, bool includeStandardValuesLinks)
    {
      return base.GetItemReferences(item, includeStandardValuesLinks);
    }

    public override ItemLink[] GetItemReferrers(Item item, bool includeStandardValuesLinks)
    {
      return base.GetItemReferrers(item, includeStandardValuesLinks);
    }

    public override ItemLink[] GetItemVersionReferrers(Item version)
    {
      Assert.ArgumentNotNull(version, nameof(version));   

      return _Map[version.Database.Name].GetItemVersionReferrers(version);
    }

    public override int GetReferenceCount([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map[item.Database.Name].GetReferenceCount(item);
    }

    public override ItemLink[] GetReferences([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map[item.Database.Name].GetReferences(item);
    }

    public override int GetReferrerCount([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map[item.Database.Name].GetReferrerCount(item);
    }

    public override ItemLink[] GetReferrers([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map[item.Database.Name].GetReferrers(item);
    }

    public override ItemLink[] GetReferrers([NotNull] Item item, [NotNull] ID sourceFieldId)
    {
      Assert.ArgumentNotNull(item, nameof(item));
      Assert.ArgumentNotNull(sourceFieldId, nameof(sourceFieldId));

      return _Map[item.Database.Name].GetReferrers(item, sourceFieldId);
    }

    [Obsolete]
    public override ItemLink[] GetReferrers([NotNull] Item item, bool deep)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map[item.Database.Name].GetReferrers(item, deep);
    }

    public override bool HasExternalReferrers([NotNull] Item item, bool deep)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map[item.Database.Name].HasExternalReferrers(item, deep);
    }

    protected override bool LinkExistsInTargetItemLinks([NotNull] ItemLink[] targetItemLinks, [NotNull] ItemLink itemLink)
    {
      Assert.ArgumentNotNull(targetItemLinks, nameof(targetItemLinks));
      Assert.ArgumentNotNull(itemLink, nameof(itemLink));

      return _Map[itemLink.SourceDatabaseName].LinkExistsInTargetItemLinks(targetItemLinks, itemLink);
    }

    public override void Rebuild([NotNull] Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      _Map[database.Name].Rebuild(database);
    }

    protected override void RemoveItemVersionLink([NotNull] ItemLink itemLink)
    {
      Assert.ArgumentNotNull(itemLink, nameof(itemLink));

      _Map[itemLink.SourceDatabaseName].RemoveItemVersionLink(itemLink);
    }

    public override void RemoveReferences([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      _Map[item.Database.Name].RemoveReferences(item);
    }

    protected override void UpdateItemVersionLinks(Item item, ItemLink[] links)
    {
      Assert.ArgumentNotNull(item, nameof(item));
      Assert.ArgumentNotNull(links, nameof(links));

      _Map[item.Database.Name].UpdateItemVersionLinks(item, links);
    }

    public override void UpdateItemVersionReferences(Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      _Map[item.Database.Name].UpdateItemVersionReferences(item);
    }

    protected override void UpdateLinks([NotNull] Item item, [NotNull] ItemLink[] links)
    {
      Assert.ArgumentNotNull(item, nameof(item));
      Assert.ArgumentNotNull(links, nameof(links));

      _Map[item.Database.Name].UpdateLinks(item, links);
    }

    public override void UpdateReferences(Item item)
    {
      _Map[item.Database.Name].UpdateReferences(item);
    }
  }
}