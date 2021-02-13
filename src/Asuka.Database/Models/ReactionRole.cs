namespace Asuka.Database.Models
{
    public class ReactionRole
    {
        public int Id { get; set; }
        public long GuildId { get; set; }
        public long MessageId { get; set; }
        public long RoleId { get; set; }
        public string EmoteName { get; set; }
    }
}
