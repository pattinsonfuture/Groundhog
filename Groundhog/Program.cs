using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Groundhog.Services;

namespace Groundhog
{
    public class Program
    {
        private readonly PluginService _pluginService;
        private readonly DatabaseService _databaseService;
        public static IConfiguration Configuration { get; private set; }
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;

                try
                {
                    Configuration = services.GetRequiredService<IConfiguration>();
                    var client = new DiscordSocketClient();

                    client.Log += Log;

                    var token = Configuration["Token"];
                    await client.LoginAsync(TokenType.Bot, token);
                    await client.StartAsync();

                    // 注册插件
                    var databaseService = new DatabaseService("your_connection_string", "your_database_name", "your_collection_name");
                    var pluginService = new PluginService(services);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                });

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}