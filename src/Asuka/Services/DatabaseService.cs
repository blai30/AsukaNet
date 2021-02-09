using System;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DbUp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asuka.Services
{
    public class DatabaseService : IHostedService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly IDbConnection _db;

        public DatabaseService(
            ILogger<DatabaseService> logger,
            IDbConnection db)
        {
            _logger = logger;
            _db = db;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Get connection string and ensure the database is created if not exists.
            var connectionString = _db.ConnectionString;
            EnsureDatabase.For.MySqlDatabase(connectionString);

            // Load sql migration scripts from assembly.
            var upgrader = DeployChanges.To
                .MySqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

            // Run database upgrade migration scripts.
            var result = upgrader.PerformUpgrade();
            if (!result.Successful)
            {
                _logger.LogError(result.Error.ToString());
                return Task.FromException(result.Error);
            }

            _logger.LogInformation("Database initialized and upgraded successfully");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
