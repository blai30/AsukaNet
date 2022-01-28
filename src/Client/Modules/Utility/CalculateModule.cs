using System;
using System.Data;
using System.Threading.Tasks;
using Asuka.Configuration;
using Asuka.Interactions;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility;

public class CalculateModule : InteractionModule
{
    private readonly DataTable _dataTable;

    public CalculateModule(
        IOptions<DiscordOptions> config,
        ILogger<CalculateModule> logger,
        DataTable dataTable) :
        base(config, logger)
    {
        _dataTable = dataTable;
    }

    [SlashCommand(
        "calculate",
        "Evaluate a mathematical expression.")]
    public async Task CalculateAsync(string expression)
    {
        // Evaluate the string expression using data table compute.
        decimal result = Convert.ToDecimal(_dataTable.Compute(expression, null));
        await RespondAsync(result.ToString("0.########"));
    }
}
