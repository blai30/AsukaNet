using Asuka.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asuka.Database.Mappers
{
    /// <summary>
    /// Build Tag model for tags table.
    /// </summary>
    public class TagMap : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            // Let database generate these values.
            builder.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
            builder.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate();

            builder.HasIndex(e => new { e.Name, e.GuildId }, "unique_per_guild").IsUnique();
        }
    }
}
