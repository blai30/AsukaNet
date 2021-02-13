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
            builder.ToTable("tags");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasColumnType("char(100)");

            builder.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("char(255)");

            builder.Property(e => e.UserId)
                .IsRequired()
                .HasColumnType("bigint");

            builder.Property(e => e.GuildId)
                .IsRequired()
                .HasColumnType("bigint");

            builder.Property(e => e.CreatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp")
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(e => new { e.Name, e.GuildId }, "unique_per_guild").IsUnique();
        }
    }
}
