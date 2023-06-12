using Discord.Commands;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groundhog.Interfaces
{
    public interface IPlugin
    {
        string Name { get; }
        string Type { get; }
        string GetName();
        // 此Plugin是否啟動，bool
        bool IsEnabled(ulong channelId);
        // 註冊指令
        Task RegisterGlobalCommands(InteractionService interactionService, IServiceProvider serviceProvider);
        //Task OnModuleLoad();
    }
}
