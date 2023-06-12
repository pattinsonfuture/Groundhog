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

        // This is the handler for the button created above. It is triggered by nmatching the customID of the button.
        [ComponentInteraction("button1")]
        public async Task ButtonHandler()
        {
            // try setting a breakpoint here to see what kind of data is supplied in a ComponentInteraction.
            var c = Context;
            await RespondAsync($"You pressed a button!");
        }

        // Simple slash command to bring up a message with a select menu
        [SlashCommand("menu", "Select Menu demo command")]
        public async Task MenuInput()
        {
            var components = new ComponentBuilder();
            // A SelectMenuBuilder is created
            var select = new SelectMenuBuilder()
            {
                CustomId = "menu1",
                Placeholder = "Select something"
            };
            // Options are added to the select menu. The option values can be generated on execution of the command. You can then use the value in the Handler for the select menu
            // to determine what to do next. An example would be including the ID of the user who made the selection in the value.
            select.AddOption("abc", "abc_value");
            select.AddOption("def", "def_value");
            select.AddOption("ghi", "ghi_value");

            components.WithSelectMenu(select);

            await RespondAsync("This message has a menu!", components: components.Build());
        }

        // SelectMenu interaction handler. This receives an array of the selections made.
        [ComponentInteraction("menu1")]
        public async Task MenuHandler(string[] selections)
        {
            // For the sake of demonstration, we only want the first value selected.
            await RespondAsync($"You selected {selections.First()}");
        }
    }
}