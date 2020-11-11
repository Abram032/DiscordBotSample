using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotSample.Data;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Commands
{
    public class Coinflip : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public Coinflip(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("coinflip")]
        [Summary("Flips a coin.")]
        public async Task ExecuteAsync()
        {
            var hex = Configuration["EmbedColorHex"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            var random = new Random();
            var result = random.NextDouble() > 0.5 ? "tails" : "heads";

            embed.WithDescription($"Coin landed on **{result}**.");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("coinflip")]
        [Summary("Flips a coin for currency.")]
        public async Task ExecuteAsync(string prediction, int bet)
        {
            var hex = Configuration["EmbedColorHex"];
            var currencyName = Configuration["CurrencyName"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            prediction = prediction.Trim().ToLower();
            if(!prediction.Equals("heads") && !prediction.Equals("tails")) {
                embed.WithDescription("Use **heads** or **tails**");
                await ReplyAsync(embed: embed.Build());
                return;
            }

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

                var random = new Random();
                var result = random.NextDouble() > 0.5 ? "tails" : "heads";

                if(result == prediction) {
                    user.Points += bet;
                    embed.WithDescription($"Coin landed on **{result}**. You won {bet} {currencyName}.");
                } else {
                    user.Points -= bet;
                    embed.WithDescription($"Coin landed on **{result}**. You lost {bet} {currencyName}.");
                }

                await _dbContext.SaveChangesAsync();
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}