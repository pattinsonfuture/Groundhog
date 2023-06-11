using Discord.Commands;
using Groundhog.Interfaces;
using System;
using System.Web;

namespace Groundhog.Plugins
{
    public class InitialPlugin : IPlugin
    {
        public string Name => "InitialPlugin";

        public string GetName()
        {
            return Name;
        }


        public bool IsEnabled(ulong channelId)
        {
            // 根据 channelId 从数据库或其他数据源获取频道是否启用插件的信息，并返回相应的布尔值
            // 实现你的逻辑...
            return true;
        }

        public async Task ExecuteAsync(SocketCommandContext context)
        {
            // 实现你的插件逻辑，包括处理 Slash Command 的逻辑
            // 使用 context 变量来获取和处理命令上下文、参数等信息
            await context.Channel.SendMessageAsync("My Plugin command executed!");
        }
    }
}
