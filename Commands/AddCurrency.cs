using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotSample.Data;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Commands
{
    public class AddCurrency : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public AddCurrency(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("add")]
        [Summary("Adds specified amount of currency to user.")]
        public async Task ExecuteAsync(int amount, SocketUser user = null) 
        {
            user = user ?? Context.User;
            var hex = Configuration["EmbedColorHex"];
            var currencyName = Configuration["CurrencyName"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(user.Username, user.GetAvatarUrl())   
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            if(amount < 0) {
                embed.WithDescription($"You can't add negative amount of {currencyName}!");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            using (var _dbContext = new BotContext())
            {
                var _user = await _dbContext.Users.FindAsync(user.Id.ToString());
                if(_user == null) {
                    embed.WithDescription("User not found!");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                _user.Points += amount;
                embed.WithDescription($"Added **{amount}** {currencyName} to {user.Mention}.");

                await _dbContext.SaveChangesAsync();
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}