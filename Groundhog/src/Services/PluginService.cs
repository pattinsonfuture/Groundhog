using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Groundhog.Interfaces;
using Groundhog.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public class PluginService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly MongoService _mongo;
    private readonly LoggingService _logger;

    public PluginService(DiscordSocketClient client, InteractionService commands, IServiceProvider services, IConfiguration configuration,MongoService mongo, LoggingService logger)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _configuration = configuration;
        _mongo = mongo;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _client.Ready += ReadyAsync;

        // Process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;

        // Process the command execution results 
        _commands.SlashCommandExecuted += SlashCommandExecuted;
        _commands.ContextCommandExecuted += ContextCommandExecuted;
        _commands.ComponentCommandExecuted += ComponentCommandExecuted;
    }


    private async Task ReadyAsync()
    {

        // 搜尋 MongoDB 中所有的Guilds
        var guilds = await _mongo.GetAllGuildsAsync();

        //{
        //    "_id": "123456789012345678",
        //    "name": "Guild Name",
        //    "plugins": [
        //        {
        //            "name": "InitialPlugin",
        //            "isEnabled": true
        //        }
        //    ]
        //}

        // 為每個 Guild 註冊有啟動的 Plugin、卸載沒有啟動的 
        foreach (var guild in guilds)
        {
            _logger.LogInfoAsync("PluginService", $"Registering commands for {guild["name"]}...").Wait();
            foreach (var plugin in guild["plugins"].AsBsonArray)
            {
                _logger.LogInfoAsync("PluginService", $"Registering {plugin["name"]}...").Wait();
                var pluginName = plugin["name"].AsString;
                var isEnabled = plugin["isEnabled"].AsBoolean;
                // pluginName 取得實例並初始化及註冊 Commands
                var pluginType = Assembly.GetExecutingAssembly().GetType($"Groundhog.Plugins.{pluginName}");
                var pluginInstance = (IPlugin)_services.GetService(pluginType);
                if (pluginType == null)
                {
                    _logger.LogErrorAsync("PluginService", $"Could not find plugin type: Groundhog.Plugins.{pluginName}").Wait();
                    continue;
                }
                if (isEnabled)
                {
                    // 初始化 Plugin
                    pluginInstance.Initialize();
                    // 安裝 Commands
                    await pluginInstance.InstallCommands();
                } else
                {
                    // 卸載 Commands
                    await pluginInstance.UninstallCommands();
                }
            }

            // 註冊到此 Guild _id
            await _commands.RegisterCommandsToGuildAsync(UInt64.Parse((string)guild["_id"]), true);
        }
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (arg.Type == InteractionType.ApplicationCommand)
                await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }

    private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        return Task.CompletedTask;
    }

    private Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        return Task.CompletedTask;
    }

}
