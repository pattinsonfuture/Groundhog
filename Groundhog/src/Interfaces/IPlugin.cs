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
        // 取得 Plugin 名稱
        string GetName();
        // 預設是否啟動
        bool DefaulEnabled();
        // 初始化
        void Initialize();
        // 安裝 Commands
        Task InstallCommands();
        // 卸載 Commands
        Task UninstallCommands();

    }
}
