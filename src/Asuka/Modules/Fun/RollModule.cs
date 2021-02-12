using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Fun
{
    [Group("roll")]
    [Remarks("Fun")]
    [Summary("Rolls a die of any number of sides. Default: 6")]
    public class RollModule : CommandModuleBase
    {
        private readonly Random _random;

        public RollModule(
            IOptions<DiscordOptions> config,
            Random random) :
            base(config)
        {
            _random = random;
        }

        [Command]
        [Remarks("roll [sides]")]
        public async Task RollAsync(int sides = 6)
        {
            // Generates a random number 0 or 1.
            var die = _random.Next(1, sides + 1);
            await ReplyAsync($":game_die: Rolled **{die}** from {sides}-sided die.");
        }
    }
}
