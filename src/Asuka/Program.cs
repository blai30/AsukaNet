using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Extensions;
using Serilog;

namespace Asuka
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine(DateTime.UtcNow);
            await CreateHostBuilder(args).Build().RunAsync();
        }

        // Typical ASP.NET host builder pattern but for console app without the web.
        // TODO: Official support planned for .NET 6.0.
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .UseSerilog()
                .UseStartup<Startup>();
    }
}
