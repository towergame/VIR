﻿using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using VIR.Services;

namespace VIR
{
    class Root
    {

        static void Main(string[] args) => new Root().MainAsync().GetAwaiter().GetResult(); //Start Main Task, this logs the bot in.

        public async Task MainAsync()
        {

            using (var services = ConfigureServices())
            {
                var __client = services.GetRequiredService<DiscordSocketClient>(); //Create a new client in the Client Variable

                __client.Log += LogAsync;

                const string tokenPath = @"token.txt";
                string botToken = File.ReadAllText(tokenPath);

                await __client.LoginAsync(TokenType.Bot, botToken); //Log in the bot
                await __client.StartAsync(); //Start the bot

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

                await Task.Delay(-1); //Prevent stopping the program until it is closed
            }
        }

        //Logging task
        private Task LogAsync(LogMessage log)
        {
            Log.Logger(Log.Logs.INFO, log.ToString());
            return Task.CompletedTask;
        }

        //Initialise all services
        private ServiceProvider ConfigureServices()
        {
            DiscordSocketClient tmp2 = new DiscordSocketClient(new DiscordSocketConfig { HandlerTimeout = null, WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance });
            CommandService tmp = new CommandService(new CommandServiceConfig{CaseSensitiveCommands = false, DefaultRunMode = RunMode.Async, }); //Command Handling Service
            return new ServiceCollection()
                .AddSingleton(tmp2) //Discord Client Service
                .AddSingleton(tmp) //Command Service
                .AddSingleton<CommandHandlingService>() //Command Handling Service
                .AddSingleton<HttpClient>() //HTTP client
                .AddSingleton<DataBaseHandlingService>() //Database Handling Service, manipulates rDB data
                .AddSingleton<AgeService>() //Age Service, sort of a wrapper for Database Handling Service
                .AddSingleton<CompanyService>()
                .BuildServiceProvider();
        }
    }
}
