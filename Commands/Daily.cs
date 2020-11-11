using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotSample.Data;
using Microsoft.Extensions.Configuration;

namespace DiscordBotSample.Commands
{
    public class Daily : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public Daily(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("daily")]
        [Summary("Recieve daily reward.")]
        public async Task ExecuteAsync() 
        {
            var random = new Random();
            var hex = Configuration["EmbedColorHex"];
            var dailyMin = int.Parse(Configuration["DailyRewardMinimum"]);
            var dailyMax = int.Parse(Configuration["DailyRewardMaximum"]);
            var currencyName = Configuration["CurrencyName"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())   
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            using (var _dbContext = new BotContext())
            {
                var _user = await _dbContext.Users.FindAsync(Context.User.Id.ToString());
                if(_user == null) {
                    embed.WithDescription("User not found!");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                var now = DateTime.Now;

                if(_user.LastDailyCollection > now.Subtract(new TimeSpan(24, 0, 0)))
                {
                    var time = _user.LastDailyCollection.AddHours(24).Subtract(now);
                    embed.WithDescription($"You can't collect daily reward yet. Try in `{time.Hours}:{time.Minutes}`");
                    await ReplyAsync(embed: embed.Build());
                    return;
                }

                var amount = random.Next(dailyMin, dailyMax + 1);
                _user.Points += amount;
                _user.LastDailyCollection = now;
                embed.WithDescription($"You are rewarded with **{amount}** {currencyName}.");

                await _dbContext.SaveChangesAsync();
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}