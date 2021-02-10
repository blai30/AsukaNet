using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asuka.Database.Models
{
    [Table("tags")]
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public int UsageCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
