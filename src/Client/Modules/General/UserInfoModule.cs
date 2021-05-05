﻿using System;
using System.Threading.Tasks;
using Asuka.Commands;
using Asuka.Configuration;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asuka.Modules.General
{
    [Group("userinfo")]
    [Alias("user")]
    [Remarks("General")]
    [Summary("Display information about a user or self.")]
    public class UserInfoModule : CommandModuleBase
    {
        public UserInfoModule(
            IOptions<DiscordOptions> config,
            ILogger<UserInfoModule> logger) :
            base(config, logger)
        {
        }

        [Command]
        [Remarks("userinfo [@user]")]
        public async Task UserInfoAsync(IUser? user = null)
        {
            // Use self if no user was specified.
            user ??= Context.User;
            string avatarUrl = user.GetAvatarUrl();

            var embed = new EmbedBuilder()
                .WithTitle("Icon direct link")
                .WithUrl(avatarUrl)
                .WithAuthor(user.ToString(), avatarUrl)
                .WithColor(Config.Value.EmbedColor)
                .WithThumbnailUrl(avatarUrl)
                .WithFooter($"Created: {user.CreatedAt.ToString("R")}")
                .AddField(
                    "ID",
                    user.Id.ToString(),
                    true)
                .AddField(
                    "Bot",
                    user.IsBot.ToString(),
                    true)
                .AddField(
                    "Presence",
                    Enum.GetName(user.Status),
                    true)
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
