using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotSample.Data;
using DiscordBotSample.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBotSample.Commands
{
    public class Balance : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public Balance(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("balance")]
        [Summary("Gets user balance.")]
        public async Task ExecuteAsync(SocketUser user = null) 
        {
            var hex = Configuration["EmbedColorHex"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();
                    
            using (var _dbContext = new BotContext())
            {
                user = user ?? Context.User;
                embed.WithAuthor(user.Username, user.GetAvatarUrl());

                var _user = await _dbContext.Users.FindAsync(user.Id.ToString());
                if(_user == null) {
                    embed.WithDescription("User not in database.");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                var currencyName = Configuration["CurrencyName"];
                
                embed.AddField("Balance", $"{_user.Points} {currencyName}");
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}