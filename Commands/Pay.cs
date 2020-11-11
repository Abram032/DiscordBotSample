using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotSample.Data;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Commands
{
    public class Pay : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public Pay(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("pay")]
        [Summary("Pay another user specified amount of currency.")]
        public async Task ExecuteAsync(int amount, SocketUser user) 
        {
            var hex = Configuration["EmbedColorHex"];
            var currencyName = Configuration["CurrencyName"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())   
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            if(amount < 0) {
                embed.WithDescription($"You can't pay negative amount of {currencyName}!");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            using (var _dbContext = new BotContext())
            {
                var _user = await _dbContext.Users.FindAsync(user.Id.ToString());
                var author = await _dbContext.Users.FindAsync(Context.User.Id.ToString());
                if(_user == null) {
                    embed.WithDescription("User not found!");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                if(author.Points < amount) {
                    embed.WithDescription($"You don't have enough {currencyName}!");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                _user.Points += amount;
                author.Points -= amount;
                
                embed.WithDescription($"You paid {user.Mention} **{amount}** {currencyName}.");

                await _dbContext.SaveChangesAsync();
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}