﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Groundhog.Interfaces;
using Groundhog.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Web;

namespace Groundhog.Plugins
{
    public class InitialPlugin : IPlugin
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public InitialPlugin()
        {
            Name = "InitialPlugin";
            Type = "Initial";
        }

        public string GetName()
        {
            return Name;
        }

        public bool IsEnabled(ulong channelId)
        {
            // 你的邏輯來決定這個插件是否應該在給定的頻道中啟用
            return true;
        }

        /// <summary>
        ///  安裝 Commands
        /// </summary>
        /// <param name="interactionService"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public async Task InstallCommands(InteractionService interactionService, IServiceProvider serviceProvider)
        {
            // 註冊 PingSlashCommand 到全域 AddModulesAsync
            await interactionService.AddModuleAsync<PingSlashCommand>(serviceProvider);

        }

        /// <summary>
        ///  卸載 Commands
        /// </summary>
        /// <param name="interactionService"></param>
        /// <returns></returns>
        public async Task UninstallCommands(InteractionService interactionService)
        {
            // 註銷 PingSlashCommand 到全域 RemoveModulesAsync
            await interactionService.RemoveModuleAsync<PingSlashCommand>();
        }

    }
}
