using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Groundhog.Interfaces;
using Groundhog.Services;
using Groundhog.Plugins.WelcomePlugin.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groundhog.Plugins.WelcomePlugin
{
    public class WelcomePlugin : IPlugin
    {
        private readonly DiscordSocketClient _client;
        private readonly LoggingService _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly MongoService _mongo;
        private readonly InteractionService _command;
        public string Name { get; private set; }

        public WelcomePlugin(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IConfiguration configuration, MongoService mongo, LoggingService logger)
        {
            _client = client;
            _logger = logger;
            _services = services;
            _configuration = configuration;
            _mongo = mongo;
            _command = commands;
            Name = "WelcomePlugin";
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
            _client.UserJoined += UserJoined;
        }

        public async Task InstallCommands()
        {
            //await _command.AddModuleAsync<WelcomeSlashCommand>(_services);
        }

        public async Task UninstallCommands()
        {
            // 在这里卸载你的命令
        }


        private async Task UserJoined(SocketGuildUser user)
        {

            // 取得公會ID
            var guildId = user.Guild.Id;
            // MongoDB 搜尋公會
            var guild = await _mongo.GetGuildAsync(guildId.ToString());
            // 搜尋name為WelcomePlugin的設定
            var welcome = guild["plugins"].AsBsonArray.Where(x => x["name"] == "WelcomePlugin").FirstOrDefault();
            // 取出useChannel的值 轉成 ulong
            var useChannelId = ulong.Parse(welcome["useChannel"].ToString());
            // 指定頻道ID，如果沒有指定，就略過
            var channel = user.Guild.GetTextChannel(useChannelId);
            if (channel != null)
            {
                // 建立一個 EmbedBuilder
                var embed = new EmbedBuilder()
                            .WithTitle($"{user.Username}#{user.Discriminator}")
                            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                            .AddField("Joined Discord on", user.CreatedAt.ToString("yyyy-MM-dd"))
                            .AddField("Joined Server on", DateTime.UtcNow.ToString("yyyy-MM-dd"))
                            .WithColor(Color.Blue)
                            .Build();
                // 歡迎 {user.Mention} 來到 {guild.Name}! // 公會名稱改為粗體
                await channel.SendMessageAsync("歡迎 " + user.Mention + " 來到 **" + user.Guild.Name + "**!", embed: embed);
            }
        }

    }

}
