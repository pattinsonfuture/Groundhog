using Discord.Interactions;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Groundhog.Services;
using Discord.WebSocket;

namespace Groundhog.SlashCommands
{
    // Must use InteractionModuleBase<SocketInteractionContext> for the InteractionService to auto-register the commands
    public class WelcomeSlashCommand : InteractionModuleBase<SocketInteractionContext>
    {
        // Inject the InteractionService into the module
        public InteractionService Commands { get; set; }
        // Inject the log into the module
        public LoggingService _logger { get; set; }

        public WelcomeSlashCommand(LoggingService logger)
        {
            _logger = logger;
        }


        [SlashCommand("guild-channels", "顯示公會內所有的頻道")]
        public async Task GuildChannels()
        {
            // 列出頻道名稱 及 ID
            await _logger.LogInfoAsync("WelcomeSlashCommand : GuildChannels", $"User: {Context.User.Username}, Command: guild-channels");
            var guild = Context.Guild;
            var channels = guild.Channels;
            var channelList = new StringBuilder();
            foreach (var channel in channels)
            {
                channelList.AppendLine($"{channel.Name} ({channel.Id})");
            }
            await RespondAsync(channelList.ToString());

        }
    }
}