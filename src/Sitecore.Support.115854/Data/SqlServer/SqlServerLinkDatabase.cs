namespace Sitecore.Support.Data.SqlServer
{
  using Sitecore.Data.Items;
  using Sitecore.Links;
  using Sitecore.Support.Data.Links;

  public class SqlServerLinkDatabase : Sitecore.Data.SqlServer.SqlServerLinkDatabase, ILinkDatabase
  {
    public SqlServerLinkDatabase(string connectionString) : base(connectionString)
    {
    }
                                      
    public new void UpdateLinks(Item item, ItemLink[] links)
    {
      base.UpdateLinks(item, links);
    }
  }
}