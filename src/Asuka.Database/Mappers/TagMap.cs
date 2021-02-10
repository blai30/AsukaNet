using Asuka.Database.Models;
using Dapper.FluentMap.Dommel.Mapping;

namespace Asuka.Database.Mappers
{
    public class TagMap : DommelEntityMap<Tag>
    {
        public TagMap()
        {
            ToTable("tags");
            Map(p => p.Id).ToColumn("id").IsKey();
            Map(p => p.Name).ToColumn("name");
            Map(p => p.Content).ToColumn("content");
            Map(p => p.UserId).ToColumn("user_id");
            Map(p => p.GuildId).ToColumn("guild_id");
            Map(p => p.UsageCount).ToColumn("usage_count").Ignore();
            Map(p => p.CreatedAt).ToColumn("created_at").Ignore();
            Map(p => p.UpdatedAt).ToColumn("updated_at").Ignore();
        }
    }
}
