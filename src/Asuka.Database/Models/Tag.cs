namespace Asuka.Database.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public int UsageCount { get; set; } = 0;
    }
}
