using System;
using System.Linq;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Database;
using Asuka.Database.Models;
using Asuka.Services;
using Discord;
using Discord.Commands;
using Humanizer;
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
        private readonly TagListenerService _service;

        public TagModule(
            IOptions<DiscordOptions> config,
            ILogger<TagModule> logger,
            IDbContextFactory<AsukaDbContext> factory,
            TagListenerService service) :
            base(config, logger)
        {
            _factory = factory;
            _service = service;
        }

        [Command("add")]
        [Alias("a", "create", "c")]
        [Remarks("tag add <name> <content> [:reaction:]")]
        [Summary("Create a new tag for the server.")]
        public async Task AddAsync(string tagName, string tagContent, IEmote reaction = null)
        {
            if (tagName.Length > 100)
            {
                await ReplyAsync("Tag name must be 100 characters or less.");
                return;
            }

            if (tagContent.Length > 255)
            {
                await ReplyAsync("Tag content must be 255 characters or less.");
                return;
            }

            var tag = new Tag
            {
                Name = tagName,
                Content = tagContent,
                Reaction = reaction?.ToString(),
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id
            };

            // Add to dictionary and database.
            await using var context = _factory.CreateDbContext();
            await context.Tags.AddAsync(tag);
            try
            {
                await context.SaveChangesAsync();
                _service.Tags.Add(tag.Id, tag);
                await ReplyAsync($"Added new tag `{tag.Name}`.");
            }
            catch
            {
                await ReplyAsync(
                    $"Error adding `{tagName.Truncate(20, "...")}`, either a tag with the same name already exists or the input parameters are invalid.");
            }
        }

        [Command("remove")]
        [Alias("r", "delete", "d")]
        [Remarks("tag remove <name>")]
        [Summary("Remove a tag from the server.")]
        public async Task RemoveAsync(string tagName)
        {
            var tag = _service.Tags.Values
                .FirstOrDefault(t =>
                    t.GuildId == Context.Guild.Id &&
                    string.Equals(t.Name, tagName, StringComparison.CurrentCultureIgnoreCase));

            // Tag does not exist.
            if (tag == null)
            {
                await ReplyAsync($"Tag `{tagName.Truncate(20, "...")}` does not exist in this server.");
                return;
            }

            // Remove from dictionary and database. Context remove will use id.
            await using var context = _factory.CreateDbContext();
            context.Tags.Remove(tag);

            try
            {
                await context.SaveChangesAsync();
                _service.Tags.Remove(tag.Id);
                await ReplyAsync($"Removed tag `{tag.Name}`.");
            }
            catch
            {
                await ReplyAsync($"Error removing tag `{tagName.Truncate(20, "...")}`.");
                throw;
            }
        }

        [Command("edit")]
        [Alias("e", "modify", "m")]
        [Remarks("tag edit <name> <content> [:reaction:]")]
        [Summary("Edit an existing tag from the server.")]
        public async Task EditAsync(string tagName, string tagContent, IEmote reaction = null)
        {
            var tag = _service.Tags.Values
                .FirstOrDefault(t =>
                    t.GuildId == Context.Guild.Id &&
                    string.Equals(t.Name, tagName, StringComparison.CurrentCultureIgnoreCase));

            // Tag does not exist.
            if (tag == null)
            {
                await ReplyAsync($"Tag `{tagName.Truncate(20, "...")}` does not exist in this server.");
                return;
            }

            // Update values in dictionary and database.
            await using var context = _factory.CreateDbContext();

            tag.Content = tagContent;
            tag.Reaction = reaction?.ToString();

            context.Tags.Attach(tag);
            context.Entry(tag).Property(t => t.Content).IsModified = true;
            context.Entry(tag).Property(t => t.Reaction).IsModified = true;

            try
            {
                await context.SaveChangesAsync();
                _service.Tags[tag.Id] = tag;
                await ReplyAsync($"Updated tag `{tag.Name}` with content `{tag.Content}`.");
            }
            catch
            {
                await ReplyAsync($"Error updating tag `{tagName.Truncate(20, "...")}`.");
                throw;
            }
        }

        [Command("list")]
        [Alias("l", "all")]
        [Remarks("tag list")]
        [Summary("List all tags from the server.")]
        public async Task ListAsync()
        {
            // Get list of tags from this guild.
            var tags = _service.Tags.Values
                .Where(t => t.GuildId == Context.Guild.Id)
                .Select(t => $"`{t.Name}`")
                .ToList();

            // Join list of tag names with comma.
            string list = string.Join(", ", tags);
            await ReplyAsync($"Tags: {list}");
        }

        [Command("info")]
        [Alias("i", "stats")]
        [Remarks("tag info <name>")]
        [Summary("Show info for a tag from the server.")]
        public async Task InfoAsync(string tagName)
        {
            var tag = _service.Tags.Values
                .FirstOrDefault(t =>
                    t.GuildId == Context.Guild.Id &&
                    string.Equals(t.Name, tagName, StringComparison.CurrentCultureIgnoreCase));

            // Tag does not exist.
            if (tag == null)
            {
                await ReplyAsync($"Tag `{tagName.Truncate(20, "...")}` does not exist in this server.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle(tag.Name)
                .WithColor(Config.Value.EmbedColor)
                .WithDescription(tag.Content)
                .AddField("Added by", Context.Client.GetUser(tag.UserId).Mention)
                .AddField("Usage count", tag.UsageCount.ToString())
                .AddField("Created", tag.CreatedAt.GetValueOrDefault().ToString("R"))
                .AddField("Last used", tag.UpdatedAt.GetValueOrDefault().ToString("R"))
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
