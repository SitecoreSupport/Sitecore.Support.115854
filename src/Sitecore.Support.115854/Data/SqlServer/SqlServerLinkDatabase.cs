namespace Sitecore.Support.Data.SqlServer
{
  using Sitecore.Data.Items;
  using Sitecore.Links;
  using Sitecore.Support.Data.Links;

  /// <summary>
  ///   SqlServerLinkDatabase serves one goal to make several protected methods public
  /// </summary>
  public class SqlServerLinkDatabase : Sitecore.Data.SqlServer.SqlServerLinkDatabase, ILinkDatabase
  {
    private ILinkDatabase _LinkDatabaseImplementation;

    public SqlServerLinkDatabase(string connectionString) : base(connectionString)
    {
    }

    public new void AddLink(Item item, ItemLink link)
    {
      base.AddLink(item, link);
    }

    public new void AddModifiedItemLinks(Item item, ItemLink[] itemLinkReferences, ItemLink[] contextitemLinks)
    {
      base.AddModifiedItemLinks(item, itemLinkReferences, contextitemLinks);
    }

    public new bool LinkExistsInTargetItemLinks(ItemLink[] targetItemLinks, ItemLink itemLink)
    {
      return base.LinkExistsInTargetItemLinks(targetItemLinks, itemLink);
    }

    public new void RemoveItemVersionLink(ItemLink itemLink)
    {
      base.RemoveItemVersionLink(itemLink);
    }

    public new void UpdateItemVersionLinks(Item item, ItemLink[] links)
    {
      base.UpdateItemVersionLinks(item, links);
    }

    public new void UpdateLinks(Item item, ItemLink[] links)
    {
      base.UpdateLinks(item, links);
    }
  }
}