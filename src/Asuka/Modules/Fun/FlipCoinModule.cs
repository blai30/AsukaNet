using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Discord.Commands;

namespace Asuka.Modules.Fun
{
    public class FlipCoinModule : CommandModuleBase
    {
        [Command("flipcoin")]
        [Summary("Flips a two-sided coin to determine heads or tails.")]
        public async Task FlipCoinAsync()
        {
            // Generates a random number 0 or 1.
            var seed = Guid.NewGuid().GetHashCode();
            var random = new Random(seed);
            var coin = random.Next(2);

            await ReplyAsync(coin == 0 ? "🪙 **Heads!**" : "🪙 **Tails!**");
        }
    }
}
