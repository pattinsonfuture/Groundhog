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
        string GetName();
        // 預設是否啟動
        bool DefaulEnabled();
        // 安裝 Commands
        Task InstallCommands(InteractionService interactionService, IServiceProvider serviceProvider);
        // 卸載 Commands
        Task UninstallCommands(InteractionService interactionService);

    }
}
