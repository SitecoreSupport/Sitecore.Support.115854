namespace Sitecore.Support.Data.SqlServer
{
  using System.Collections.Generic;
  using System.Linq;
  using Sitecore.Configuration;
  using Sitecore.Support.Data.Links;

  public class SwitchingSqlServerLinkDatabase : SwitchingLinkDatabase
  {                       
    [UsedImplicitly]
    public SwitchingSqlServerLinkDatabase() : base(CreateMap())
    {
    }

    [NotNull]
    private static Dictionary<string, ILinkDatabase> CreateMap()
    {
      return Factory.GetDatabases()
        .ToDictionary(
          x => x.Name,
          x => x.ConnectionStringName)
        .Where(x => !string.IsNullOrEmpty(x.Value))
        .ToDictionary(
          x => x.Key,
          x => (ILinkDatabase)new SqlServerLinkDatabase(Settings.GetConnectionString(x.Value)));
    }
  }
}