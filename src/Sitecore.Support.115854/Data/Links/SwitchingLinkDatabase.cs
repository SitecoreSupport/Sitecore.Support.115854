namespace Sitecore.Support.Data.Links
{
  using System;
  using System.Collections.Generic;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
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

    [Obsolete]
    public override ItemLink[] GetReferrers([NotNull] Item item, bool deep)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map[item.Database.Name].GetReferrers(item, deep);
    }

    public override void RemoveReferences([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      _Map[item.Database.Name].RemoveReferences(item);
    }

    protected override void UpdateLinks([NotNull] Item item, [NotNull] ItemLink[] links)
    {
      Assert.ArgumentNotNull(item, nameof(item));
      Assert.ArgumentNotNull(links, nameof(links));

      _Map[item.Database.Name].UpdateLinks(item, links);
    }
  }
}