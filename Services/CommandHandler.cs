using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Services {
    public class CommandHandler {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;
        public CommandHandler(DiscordSocketClient client, CommandService commands, IConfiguration configuration, IServiceProvider services)
        {
            _commands = commands;
            _client = client;
            _configuration = configuration;
            _services = services;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {      
            var message = messageParam as SocketUserMessage;
            if (message == null) {
                return;
            }

            int argPos = 0;

            if(!message.HasStringPrefix(_configuration["CommandPrefix"], ref argPos) || message.Author.IsBot) {
                return;
            }
            
            var context = new SocketCommandContext(_client, message);

            var result = await _commands.ExecuteAsync(context, argPos, services: _services);
        }
    }
}