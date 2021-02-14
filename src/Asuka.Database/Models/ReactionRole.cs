namespace Asuka.Database.Models
{
    public class ReactionRole
    {
        public int Id { get; set; }
        public ulong GuildSnowflake { get; set; }
        public ulong MessageSnowflake { get; set; }
        public ulong RoleSnowflake { get; set; }
        public string Emote { get; set; }
    }
}
