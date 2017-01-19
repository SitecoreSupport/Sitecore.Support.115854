namespace Sitecore.Support.Data.SqlServer
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Links;

  [UsedImplicitly]
  public class SwitchingParallelSqlServerLinkDatabase : LinkDatabase
  {
    [NotNull]
    private readonly Lazy<IReadOnlyDictionary<string, ParallelSqlServerLinkDatabase>> _Map = new Lazy<IReadOnlyDictionary<string, ParallelSqlServerLinkDatabase>>(() => CreateMap());

    public override void Compact([NotNull] Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      _Map.Value[database.Name].Compact(database);
    }

    public override ItemLink[] GetBrokenLinks([NotNull] Database database)
    {
      Assert.ArgumentNotNull(database, nameof(database));

      return _Map.Value[database.Name].GetBrokenLinks(database);
    }

    public override int GetReferenceCount([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map.Value[item.Database.Name].GetReferenceCount(item);
    }

    public override ItemLink[] GetReferences([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map.Value[item.Database.Name].GetReferences(item);
    }

    public override int GetReferrerCount([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map.Value[item.Database.Name].GetReferrerCount(item);
    }

    public override ItemLink[] GetReferrers([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map.Value[item.Database.Name].GetReferrers(item);
    }

    public override ItemLink[] GetReferrers([NotNull] Item item, bool deep)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      return _Map.Value[item.Database.Name].GetReferrers(item);
    }

    public override void RemoveReferences([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, nameof(item));

      _Map.Value[item.Database.Name].RemoveReferences(item);
    }

    protected override void UpdateLinks([NotNull] Item item, [NotNull] ItemLink[] links)
    {
      Assert.ArgumentNotNull(item, nameof(item));
      Assert.ArgumentNotNull(links, nameof(links));

      _Map.Value[item.Database.Name].UpdateLinks(item, links);
    }

    private static Dictionary<string, ParallelSqlServerLinkDatabase> CreateMap()
    {
      return Factory.GetDatabases()
        .ToDictionary(
          x => x.Name,
          x => x.ConnectionStringName)
        .Where(x => !string.IsNullOrEmpty(x.Value))
        .ToDictionary(
          x => x.Key,
          x => new ParallelSqlServerLinkDatabase(Settings.GetConnectionString(x.Value)));
    }
  }
}