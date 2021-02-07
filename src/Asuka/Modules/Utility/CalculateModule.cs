using System;
using System.Data;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord.Commands;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Utility
{
    [Group("calculate")]
    [Alias("math", "solve")]
    [Remarks("Utility")]
    [Summary("Evaluate a mathematical expression.")]
    public class CalculateModule : CommandModuleBase
    {
        private readonly DataTable _dataTable;

        public CalculateModule(
            IOptions<DiscordOptions> config,
            DataTable dataTable) :
            base(config)
        {
            _dataTable = dataTable;
        }

        [Command]
        [Remarks("calculate <expression>")]
        public async Task CalculateAsync([Remainder] string expression)
        {
            // Evaluate the string expression using data table compute.
            decimal result = Convert.ToDecimal(_dataTable.Compute(expression, null));
            await ReplyAsync(result.ToString("0.########"));
        }
    }
}
