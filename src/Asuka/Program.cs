using System;
using System.Threading.Tasks;

namespace Asuka
{
    public class Program
    {
        public static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

        private async Task MainAsync(string[] args)
        {
            Console.WriteLine(DateTime.UtcNow);
            await new Startup(args).RunAsync();
        }
    }
}
