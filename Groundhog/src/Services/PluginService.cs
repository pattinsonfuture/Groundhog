﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Groundhog.Interfaces;
using System.Reflection;

public class PluginService
{
    private readonly List<IPlugin> _plugins;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;

    public PluginService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
    {
        _plugins = new List<IPlugin>();
        _client = client;
        _commands = commands;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        // Process the InteractionCreated payloads to execute Interactions commands
        _client.InteractionCreated += HandleInteraction;

        // Process the command execution results 
        _commands.SlashCommandExecuted += SlashCommandExecuted;
        _commands.ContextCommandExecuted += ContextCommandExecuted;
        _commands.ComponentCommandExecuted += ComponentCommandExecuted;
    }


    public void AddPlugin(IPlugin plugin)
    {
        _plugins.Add(plugin);
    }

    public async Task RegisterAllCommands()
    {
        // 打印所有插件的名稱
        Console.WriteLine("Registering all commands...");
        foreach (var plugin in _plugins)
        {
            Console.WriteLine($"Registering commands for {plugin.GetName()}...");
            await plugin.RegisterGlobalCommands(_commands, _services);

        }
            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
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
}