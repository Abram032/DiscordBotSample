using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotSample.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private IConfiguration Configuration { get; set; }
        private CommandService CommandService { get; set; }
        public Help(IConfiguration configuration, CommandService commandService)
        {
            Configuration = configuration;
            CommandService = commandService;
        }

        [Command("help")]
        [Summary("Shows help.")]
        public async Task ExecuteAsync()
        {
            var hex = Configuration["EmbedColorHex"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                .AddField("Available commands:", String.Join(", ", GetCommandNames()))
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("help")]
        [Summary("Shows help for a command.")]
        public async Task ExecuteAsync(string command)
        {
            var hex = Configuration["EmbedColorHex"];
            var prefix = Configuration["CommandPrefix"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())           
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            var result = CommandService.Search(Context, command);

            if(!result.IsSuccess) {
                embed.AddField($"Command: `{prefix}{command}`", "Command not found.");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            foreach(var info in result.Commands) 
            {
                var parameters = info.Command.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : $"{p.Name}").ToList();
                var parametersString = parameters.Count != 0 ? $" {String.Join(' ', parameters)}" : "";
                embed.AddField($"Command: `{prefix}{info.Command.Name}{parametersString}`", $"{info.Command.Summary}");
                embed.AddField($"Aliases:", String.Join(", ", info.Command.Aliases.Select(str => $"`{prefix}{str}`")));
                if(parameters.Count != 0) {
                    embed.AddField($"Parameters:", String.Join(' ', parameters));
                }
            }

            await ReplyAsync(embed: embed.Build());
        }

        private List<string> GetCommandNames() {
            var prefix = Configuration["CommandPrefix"];
            return CommandService.Commands.Select(p => $"`{prefix}{p.Name}`").Distinct().ToList();
        }
    }
}