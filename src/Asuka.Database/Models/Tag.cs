using System;

namespace Asuka.Database.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Reaction { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int UsageCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
