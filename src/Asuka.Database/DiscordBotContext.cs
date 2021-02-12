using Asuka.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Asuka.Database
{
    public sealed class DiscordBotContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        public DiscordBotContext(DbContextOptions<DiscordBotContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>()
                .Ignore("CreatedAt")
                .Ignore("UpdatedAt");
        }
    }
}
