using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Groundhog.Services
{
    public class PluginService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private DatabaseService _databaseService;


        public PluginService(IServiceProvider services)
        {
            // 获取 DatabaseService 实例
            _databaseService = services.GetRequiredService<DatabaseService>();
            _services = services;
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandService = _services.GetRequiredService<CommandService>();

        }

        public async Task InstallPluginsAsync()
        {
            // 获取已启用的插件列表
            var enabledPlugins = await _databaseService.GetEnabledPluginsAsync();

            // 安装每个启用的插件
            foreach (var plugin in enabledPlugins)
            {
                // 使用反射获取插件类型
                var pluginType = Assembly.GetEntryAssembly().GetType(plugin.Type);

                // 创建插件实例
                var pluginInstance = ActivatorUtilities.CreateInstance(_services, pluginType);

                // 注册插件中的Slash指令模块
                await _commandService.AddModulesAsync(pluginType.Assembly, _services);

                // 将插件实例传递给每个模块
                foreach (var module in _commandService.Modules.Where(m => m.ModuleType.Assembly == pluginType.Assembly))
                {
                    module.ModuleType.GetProperty("PluginInstance")?.SetValue(module, pluginInstance);
                }
            }
        }

        public void Start()
        {
            // 启动 Change Stream
            _databaseService.StartChangeStream();
        }

        public void Stop()
        {
            // 停止 Change Stream
            _databaseService.StopChangeStream();
        }
    }

}
