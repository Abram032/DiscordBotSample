using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DiscordBotSample.Data;

namespace DiscordBotSample.Commands
{
    public class Roll : ModuleBase<SocketCommandContext>
    {
        private IConfiguration Configuration { get; set; }
        public Roll(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("roll")]
        [Summary("Rolls a d-sized dice. Default size is d6.")]
        public async Task ExecuteAsync(int size = 6)
        {
            var random = new Random();
            var number = random.Next(1, size + 1);

            var hex = Configuration["EmbedColorHex"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                .WithDescription($"You rolled {number}.")
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("roll")]
        [Summary("Rolls a d6 dice for currency.")]
        public async Task ExecuteAsync2(int prediction, int bet)
        {
            var random = new Random();
            var number = random.Next(1, 7);
            var result = number == prediction;
            var currencyName = Configuration["CurrencyName"];

            var hex = Configuration["EmbedColorHex"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            if(bet < 0) {
                embed.WithDescription($"You can't bet negative amount of {currencyName}!");
                await ReplyAsync(embed: embed.Build());
                return;
            }

            using (var _dbContext = new BotContext())
            {
                var user = await _dbContext.Users.FindAsync(Context.User.Id.ToString());
                if(user == null) {
                    embed.WithDescription("User not found!");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                if(user.Points < bet) {
                    embed.WithDescription($"Get some more {currencyName}.");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                if(result) {
                    user.Points += bet;
                    embed.WithDescription($"You rolled **{number}**. You won {bet} {currencyName}.");
                } else {
                    user.Points -= bet;
                    embed.WithDescription($"You rolled **{number}**. You lost {bet} {currencyName}.");
                }

                await _dbContext.SaveChangesAsync();
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}