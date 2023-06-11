using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groundhog.Interfaces
{
    public interface IPlugin
    {
        // 在接口中定义插件所需的方法和属性
        string Name { get; }
        // 此Plugin是否啟動，function固定回傳bool
        string GetName();
        bool IsEnabled(ulong channelId);
        // 定義任務類型
        Task ExecuteAsync(SocketCommandContext context);
    }
}
