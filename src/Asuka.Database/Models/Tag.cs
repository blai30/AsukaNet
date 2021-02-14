using System;

namespace Asuka.Database.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ulong GuildSnowflake { get; set; }
        public ulong UserSnowflake { get; set; }
        public int UsageCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
