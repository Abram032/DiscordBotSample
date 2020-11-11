using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotSample.Data;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Commands
{
    public class SetCurrency : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public SetCurrency(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("set")]
        [Summary("Sets specified amount of currency on user.")]
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

            using (var _dbContext = new BotContext())
            {
                var _user = await _dbContext.Users.FindAsync(user.Id.ToString());
                if(_user == null) {
                    embed.WithDescription("User not found!");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                _user.Points = amount;
                embed.WithDescription($"Set **{amount}** {currencyName} for {user.Mention}.");

                await _dbContext.SaveChangesAsync();
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}