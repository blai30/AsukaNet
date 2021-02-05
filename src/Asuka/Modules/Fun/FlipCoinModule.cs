using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Fun
{
    [Group("flipcoin")]
    [Remarks("Fun")]
    [Summary("Flips a two-sided coin to determine heads or tails.")]
    public class FlipCoinModule : CommandModuleBase
    {
        private readonly Random _random;

        public FlipCoinModule(
            IOptions<DiscordOptions> config,
            Random random)
            : base(config)
        {
            _random = random;
        }

        [Command]
        [Name("")]
        public async Task FlipCoinAsync()
        {
            // Generates a random number 0 or 1.
            var coin = _random.Next(2);
            await ReplyAsync(coin == 0 ? ":coin: **Heads!**" : ":coin: **Tails!**");
        }
    }
}
