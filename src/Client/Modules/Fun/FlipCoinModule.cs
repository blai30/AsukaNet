using System;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Fun;

public class FlipCoinModule : InteractionModule
{
    private readonly Random _random;

    public FlipCoinModule(
        IOptions<DiscordOptions> config,
        ILogger<FlipCoinModule> logger,
        Random random) :
        base(config, logger)
    {
        _random = random;
    }

    [SlashCommand(
        "flipcoin",
        "Flips a two-sided coin to determine heads or tails.")]
    public async Task FlipCoinAsync()
    {
        // Generates a random number 0 or 1.
        int coin = _random.Next(2);
        await RespondAsync(coin == 0 ? ":coin: **Heads!**" : ":coin: **Tails!**");
    }
}
