using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Commands
{
    public class Say : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public Say(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("say")]
        [Summary("Echoes a message.")]
        public async Task ExecuteAsync(string message)
        {
            var hex = Configuration["EmbedColorHex"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                .WithDescription(message)
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();
                
            await ReplyAsync(embed: embed.Build());
        }
    }
}
