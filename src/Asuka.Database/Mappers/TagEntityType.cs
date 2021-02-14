using Asuka.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asuka.Database.Mappers
{
    /// <summary>
    /// Build Tag model for tags table.
    /// </summary>
    public class TagEntityType : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate();

            builder.HasIndex(e => new { e.Name, e.GuildId }, "unique_per_guild").IsUnique();
        }
    }
}
