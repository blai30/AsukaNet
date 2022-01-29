using System;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules;

public class RollModule : InteractionModule
{
    private readonly Random _random;

    public RollModule(
        IOptions<DiscordOptions> config,
        ILogger<RollModule> logger,
        Random random) :
        base(config, logger)
    {
        _random = random;
    }

    [SlashCommand(
        "roll",
        "Rolls a die of any number of sides. Default: 6.")]
    public async Task RollAsync(int sides = 6)
    {
        // Generates a random number 0 or 1.
        int die = _random.Next(1, sides + 1);
        await RespondAsync($":game_die: Rolled **{die.ToString()}** from {sides.ToString()}-sided die.");
    }
}
