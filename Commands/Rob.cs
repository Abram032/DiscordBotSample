using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotSample.Data;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Commands
{
    public class Rob : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public Rob(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("rob")]
        [Summary("Robs specified user.")]
        public async Task ExecuteAsync(SocketUser user) 
        {
            var random = new Random();
            var hex = Configuration["EmbedColorHex"];
            var currencyName = Configuration["CurrencyName"];
            var maxRobAmount = int.Parse(Configuration["MaxRobAmount"]);
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())   
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            if(user.Id == Context.User.Id) {
                embed.WithDescription($"You can't rob yourself.");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            using (var _dbContext = new BotContext())
            {
                var victim = await _dbContext.Users.FindAsync(user.Id.ToString());
                var thief = await _dbContext.Users.FindAsync(Context.User.Id.ToString());
                if(victim == null) {
                    embed.WithDescription("User not found!");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                var chances = random.NextDouble();
                if(chances < 0.1) {
                    embed.WithTitle("Successful robbery");
                    embed.WithDescription($"You found nothing. Good job.");
                }
                else if(chances < 0.45) {
                    var amount = random.Next(0, maxRobAmount);
                    victim.Points -= amount;
                    thief.Points += amount;

                    embed.WithTitle("Successful robbery");
                    embed.WithDescription($"You manged to rob {amount} {currencyName}.");
                }
                else {
                    var amount = random.Next(0, maxRobAmount);
                    thief.Points -= amount;

                    embed.WithTitle("Failed robbery");
                    embed.WithDescription($"Your victim's grandma shot you with her shotgun. You paid {amount} for hospital treatment.");
                }

                await _dbContext.SaveChangesAsync();
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}