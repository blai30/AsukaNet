using System.Linq;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Database;
using Asuka.Database.Models;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Tags
{
    [Group("tag")]
    [Remarks("Tags")]
    [Summary("Add, edit, or delete tags.")]
    [RequireContext(ContextType.Guild)]
    public class TagModule : CommandModuleBase
    {
        private readonly IDbContextFactory<AsukaDbContext> _factory;

        public TagModule(
            IOptions<DiscordOptions> config,
            ILogger<TagModule> logger,
            IDbContextFactory<AsukaDbContext> factory) :
            base(config, logger)
        {
            _factory = factory;
        }

        [Command("add")]
        [Alias("a", "create", "c")]
        [Remarks("tag add <name> <content>")]
        [Summary("Create a new tag for the server.")]
        public async Task AddAsync(string tagName, string tagContent)
        {
            var tag = new Tag
            {
                Name = tagName,
                Content = tagContent,
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id
            };

            await using var context = _factory.CreateDbContext();
            await context.AddAsync(tag);
            try
            {
                await context.SaveChangesAsync();
                await ReplyAsync($"Added new tag `{tag.Name}`.");
            }
            catch
            {
                await ReplyAsync(
                    $"Error adding `{tagName}`, either a tag with the same name already exists or the input parameters are invalid.");
            }
        }

        [Command("remove")]
        [Alias("r", "delete", "d")]
        [Remarks("tag remove <name>")]
        [Summary("Remove a tag from the server.")]
        public async Task RemoveAsync(string tagName)
        {
            await using var context = _factory.CreateDbContext();

            // Get tag by name.
            var tag = await context.Tags.AsQueryable()
                .FirstOrDefaultAsync(t => t.Name == tagName);

            context.Tags.Remove(tag);

            try
            {
                await context.SaveChangesAsync();
                await ReplyAsync($"Removed tag `{tag.Name}`.");
            }
            catch
            {
                await ReplyAsync($"Error removing tag `{tagName}`.");
                throw;
            }
        }

        [Command("edit")]
        [Alias("e", "modify", "m")]
        [Remarks("tag edit <name> <content>")]
        [Summary("Edit an existing tag from the server.")]
        public async Task EditAsync(string tagName, string tagContent)
        {
            await using var context = _factory.CreateDbContext();

            // Get tag by name.
            var tag = await context.Tags.AsQueryable()
                .FirstOrDefaultAsync(t => t.Name == tagName);

            tag.Content = tagContent;

            try
            {
                await context.SaveChangesAsync();
                await ReplyAsync($"Updated tag `{tag.Name}` with content `{tag.Content}`.");
            }
            catch
            {
                await ReplyAsync($"Error updating tag `{tagName}`.");
                throw;
            }
        }

        [Command("list")]
        [Alias("l", "all")]
        [Remarks("tag list")]
        [Summary("List all tags from the server.")]
        public async Task ListAsync()
        {
            await using var context = _factory.CreateDbContext();

            // Get list of tags from this guild.
            var tags = await context.Tags.AsNoTracking()
                .Where(tag => tag.GuildId == Context.Guild.Id)
                .Select(tag => $"`{tag.Name}`")
                .ToListAsync();

            // Join list of tag names with comma.
            string list = string.Join(", ", tags);
            await ReplyAsync("Tags: " + list);
        }

        [Command("info")]
        [Alias("i", "stats")]
        [Remarks("tag info <name>")]
        [Summary("Show info for a tag from the server.")]
        public async Task InfoAsync(string tagName)
        {
            await using var context = _factory.CreateDbContext();

            // Get tag by name.
            var tag = await context.Tags.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Name == tagName);

            if (tag == null)
            {
                await ReplyAsync($"Tag `{tagName}` does not exist in this server.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle(tag.Name)
                .WithColor(Config.Value.EmbedColor)
                .WithDescription(tag.Content)
                .AddField("Added by", Context.Client.GetUser(tag.UserId).Mention)
                .AddField("Usage count", tag.UsageCount)
                .AddField("Created", tag.CreatedAt.GetValueOrDefault().ToString("R"))
                .AddField("Last used", tag.UpdatedAt.GetValueOrDefault().ToString("R"))
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
