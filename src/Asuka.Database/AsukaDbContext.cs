using System.Reflection;
using Asuka.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Asuka.Database
{
    public sealed class AsukaDbContext : DbContext
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbSet<Tag> Tags { get; set; }
        public DbSet<ReactionRole> ReactionRoles { get; set; }

        public AsukaDbContext(DbContextOptions<AsukaDbContext> options, IServiceScopeFactory scopeFactory) : base(options)
        {
            _scopeFactory = scopeFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            using var scope = _scopeFactory.CreateScope();
            string connectionString = scope.ServiceProvider
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("Docker");

            optionsBuilder
                .UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString))
                // Map PascalCase POCO properties to snake_case MySQL tables and columns.
                .UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensure correct collation.
            modelBuilder.UseCollation("utf8mb4_unicode_ci");

            // Load entity type configuration mappers.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
