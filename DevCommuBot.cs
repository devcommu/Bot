using System;
using System.Threading.Tasks;

using DevCommuBot.Data;
using DevCommuBot.Services;

using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

using Serilog;

namespace DevCommuBot
{
    internal class DevCommuBot
    {
        private IConfigurationRoot config;

        public async Task StartAsync()
        {
            config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json").Build();
            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new()
                {
                    LogLevel = Discord.LogSeverity.Verbose,
                    AlwaysDownloadUsers = true,
                    MessageCacheSize = 100,
                    GatewayIntents = Discord.GatewayIntents.All
                }))
                .AddSingleton(config)
                .AddSingleton(new CommandService(new()
                {
                    DefaultRunMode = Discord.Commands.RunMode.Async,
                    LogLevel = Discord.LogSeverity.Verbose,
                    CaseSensitiveCommands = false,
                    ThrowOnError = false,
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<LoggerService>()
                .AddSingleton<UtilService>()
                .AddSingleton<GuildService>()
                .AddDbContext<DataContext>()
                .AddSingleton<DataService>()
                .AddSingleton<PointService>()
                .AddSingleton<InteractionService>()
                .AddSingleton<ApexStatsService>();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<LoggerService>();

            //Start bot
            await serviceProvider.GetRequiredService<StartupService>().StartAsync();
            serviceProvider.GetRequiredService<CommandHandler>();

            serviceProvider.GetRequiredService<GuildService>();
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            //Add SeriLog
            services.AddLogging((configure) => configure.AddSerilog());
            //Remove default HttpClient logging as it is extremely verbose
            services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            //Configure logging level
            var logLevel = "debug";
            var level = Serilog.Events.LogEventLevel.Error;
            if (!string.IsNullOrEmpty(logLevel))
            {
                switch (logLevel.ToLower())
                {
                    case "error":
                        {
                            level = Serilog.Events.LogEventLevel.Error;
                            break;
                        }
                    case "info":
                        {
                            level = Serilog.Events.LogEventLevel.Information;
                            break;
                        }
                    case "debug":
                        {
                            level = Serilog.Events.LogEventLevel.Debug;
                            break;
                        }
                    case "crit":
                        {
                            level = Serilog.Events.LogEventLevel.Fatal;
                            break;
                        }
                    case "warn":
                        {
                            level = Serilog.Events.LogEventLevel.Warning;
                            break;
                        }
                    case "trace":
                        {
                            level = Serilog.Events.LogEventLevel.Debug;
                            break;
                        }
                }
            }
            Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .MinimumLevel.Is(level)
                    .CreateLogger();
        }
    }
}