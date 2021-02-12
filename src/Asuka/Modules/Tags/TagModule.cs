using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Asuka.Database;
using Asuka.Database.Models;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.Tags
{
    [Group("tag")]
    [Remarks("Tags")]
    [Summary("Add, edit, or delete tags.")]
    [RequireContext(ContextType.Guild)]
    public class TagModule : CommandModuleBase
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TagModule(
            IOptions<DiscordOptions> config,
            IServiceScopeFactory scopeFactory) :
            base(config)
        {
            _scopeFactory = scopeFactory;
        }

        [Command("add")]
        [Alias("a", "create", "c")]
        [Remarks("Create a new tag for the server.")]
        public async Task AddAsync(string tagName, string tagContent)
        {
            var tag = new Tag
            {
                Name = tagName,
                Content = tagContent,
                UserId = Context.User.Id,
                GuildId = Context.Guild.Id
            };

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DiscordBotContext>();

            await context.Tags.AddAsync(tag);
            await context.SaveChangesAsync();
            await ReplyAsync($"Added new tag `{tag.Name}`.");
        }

        [Command("edit")]
        [Alias("e", "modify", "m")]
        [Remarks("Edit an existing tag from the server.")]
        public async Task EditAsync(string tagName, string tagContent)
        {
            await Task.CompletedTask;
        }

        [Command("get")]
        [Alias("g", "fetch", "f")]
        [Remarks("Get an existing tag from the server.")]
        public async Task GetAsync(string tagName)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DiscordBotContext>();
            var tag = await context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);

            if (tag == null)
            {
                await ReplyAsync($"Tag `{tagName}` does not exist. .·´¯`(>▂<)´¯`·. ");
                return;
            }

            await ReplyAsync(tag.Content);
        }

        [Command("remove")]
        [Alias("r", "delete", "d")]
        [Remarks("Remove a tag from the server.")]
        public async Task RemoveAsync(string tagName)
        {
            await Task.CompletedTask;
        }

        [Command("list")]
        [Alias("l", "all")]
        [Remarks("List all tags from the server.")]
        public async Task ListAsync()
        {
            await Task.CompletedTask;
        }

        [Command("info")]
        [Alias("i", "stats")]
        [Remarks("Show info for a tag from the server.")]
        public async Task InfoAsync(string tagName)
        {
            await Task.CompletedTask;
        }
    }
}
