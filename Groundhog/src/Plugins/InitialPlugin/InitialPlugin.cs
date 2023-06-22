using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Groundhog.Interfaces;
using Groundhog.Services;
using Groundhog.Plugins.InitialPlugin.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Web;

namespace Groundhog.Plugins.InitialPlugin
{
    public class InitialPlugin : IPlugin
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _command;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly MongoService _mongo;
        private readonly LoggingService _logger;
        public string Name { get; private set; }

        public InitialPlugin(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IConfiguration configuration, MongoService mongo, LoggingService logger)
        {
            _client = client;
            _command = commands;
            _services = services;
            _configuration = configuration;
            _mongo = mongo;
            _logger = logger;

            Name = "InitialPlugin";
        }

        public string GetName()
        {
            return Name;
        }

        public bool DefaulEnabled()
        {
            return true;
        }

        public void Initialize()
        {
        }

        /// <summary>
        ///  安裝 Commands
        /// </summary>
        public async Task InstallCommands()
        {
            // 註冊 PingSlashCommand 到全域 AddModulesAsync
            await _command.AddModuleAsync<PingSlashCommand>(_services);

        }

        /// <summary>
        ///  卸載 Commands
        /// </summary>
        public async Task UninstallCommands()
        {
            // 註銷 PingSlashCommand 到全域 RemoveModulesAsync
            await _command.RemoveModuleAsync<PingSlashCommand>();
        }

    }
}
