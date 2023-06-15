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
    public class PingSlashCommand : InteractionModuleBase<SocketInteractionContext>
    {
        // Inject the InteractionService into the module
        public InteractionService Commands { get; set; }
        // Inject the log into the module
        public LoggingService _logger { get; set; }

        public PingSlashCommand(LoggingService logger)
        {
            _logger = logger;
        }


        // Basic slash command. [SlashCommand("name", "description")]
        // Similar to text command creation, and their respective attributes
        [SlashCommand("ping", "Receive a pong!")]
        public async Task Ping()
        {
            // New LogMessage created to pass desired info to the console using the existing Discord.Net LogMessage parameters
            await _logger.LogInfoAsync("PingSlashCommand : Ping", $"User: {Context.User.Username}, Command: ping");
            // Respond to the user
            await RespondAsync("pong");
        }

        // Simple slash command to bring up a message with a button to press
        [SlashCommand("button", "Button demo command")]
        public async Task ButtonInput()
        {
            var components = new ComponentBuilder();
            var button = new ButtonBuilder()
            {
                Label = "Button",
                CustomId = "button1",
                Style = ButtonStyle.Primary
            };

            // Messages take component lists. Either buttons or select menus. The button can not be directly added to the message. It must be added to the ComponentBuilder.
            // The ComponentBuilder is then given to the message components property.
            components.WithButton(button);

            await RespondAsync("This message has a button!", components: components.Build());
        }
    }
}