using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Linq;
using DiscordBotSample.Data;
using DiscordBotSample.Models;

namespace DiscordBotSample.Services {
    public class BotService : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly CommandHandler _handler;
        private readonly IConfiguration _configuration;

        public BotService(DiscordSocketClient client, CommandService commands, 
            CommandHandler handler, IConfiguration configuration)
        {
            _client = client;
            _commands = commands;
            _handler = handler;
            _configuration = configuration;
        }

        public void Dispose()
        {
            
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Log += Log;
            _client.Ready += Ready;
            _client.UserJoined += UserJoined;
            await _handler.InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, _configuration["Token"]);
            await _client.StartAsync();
            
            await Task.Delay(-1);
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            using(var context = new BotContext())
            {
                var _user = await context.Users.FindAsync(user.Id.ToString());
                if(_user == null) {
                    await context.Users.AddAsync(new User {
                        Id = user.Id.ToString(),
                        Points = int.Parse(_configuration["InitialCurrency"])
                    });
                }
            }
        }

        private async Task Ready()
        {
            using(var context = new BotContext())
            {
                var users = _client.Guilds.First().Users;
                foreach(var user in users) {
                    var _user = await context.Users.FindAsync(user.Id.ToString());
                    if(_user == null) {
                        await context.Users.AddAsync(new User {
                            Id = user.Id.ToString(),
                            Points = int.Parse(_configuration["InitialCurrency"])
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            Console.WriteLine("Users downloaded and added to database.");
            Console.WriteLine("Bot is ready!");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}