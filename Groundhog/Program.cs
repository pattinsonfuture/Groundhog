﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Groundhog.Services;
using Microsoft.Extensions.Logging;
using Discord.Interactions;
using Groundhog.Interfaces;
using Groundhog.Plugins;
using System.Reflection;

namespace Groundhog
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static Task Main() => new Program().MainAsync();
        public async Task MainAsync()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // 讀取 appsettings.json
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();

                    // 獲取正确的 Configuration 对象
                    Configuration = config.Build();
                })
                .ConfigureServices(services =>
                {
                    // 註冊 LogginService 實例
                    services.AddSingleton<LoggingService>();
                    // 註冊 DatabaseService 實例
                    services.AddSingleton<MongoService>();

                    // 註冊 DiscordSocketClient 實例
                    services.AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                        AlwaysDownloadUsers = true,
                    }));

                    // 註冊 InteractionService  // Used for slash commands and their registration with Discord
                    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

                    // 註冊 PluginService
                    services.AddSingleton<PluginService>();

                    // 獲取所有插件類型
                    var pluginTypes = Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IPlugin)) && !t.IsAbstract);

                    // 將所有插件註冊為 Transient 服務
                    foreach (var pluginType in pluginTypes)
                    {
                        services.AddSingleton(pluginType);
                    }
                })
                .Build();

            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            // 獲取 DiscordSocketClient 實例
            var _client = provider.GetRequiredService<DiscordSocketClient>();
            // 獲取 LoggingService 實例
            var _logger = provider.GetRequiredService<LoggingService>();
            // 獲取 MongoDB 實例
            var _mongoService = provider.GetRequiredService<MongoService>();
            // 獲取 InteractionService 實例
            var _commands = provider.GetRequiredService<InteractionService>();
            // 獲取 PluginService 實例
            var _pluginService = provider.GetRequiredService<PluginService>();
            // 啟動 Change Stream
            //_mongoService.StartChangeStream();

            // 獲取 PluginService 實例，並初始化
            await _pluginService.InitializeAsync();

            // Subscribe to client log events
            _client.Log += async (LogMessage msg) => provider.GetRequiredService<LoggingService>().LogAsync(msg);
            // Subscribe to slash command log events
            _commands.Log += async (LogMessage msg) => provider.GetRequiredService<LoggingService>().LogAsync(msg);


            // Login and connect to Discord.
            await _client.LoginAsync(TokenType.Bot, Configuration["DiscordToken"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

    }
}