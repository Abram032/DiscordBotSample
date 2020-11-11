using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotSample.Data;
using DiscordBotSample.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;

namespace DiscordBotSample.Commands
{
    public class Leaderboard : ModuleBase<SocketCommandContext>
    {
        public IConfiguration Configuration { get; set; }
        public DiscordSocketClient Client { get; set; }
        public Leaderboard(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [Command("leaderboard")]
        [Summary("Shows top 10 leaderboard or position for specified user.")]
        public async Task ExecuteAsync(SocketUser user = null) 
        {
            var hex = Configuration["EmbedColorHex"];
            var currencyName = Configuration["CurrencyName"];
            var embed = new EmbedBuilder()
                .WithColor(new Color(Convert.ToUInt32(hex, 16)))
                .WithAuthor(Context.User.Username, Context.User.GetAvatarUrl())
                .WithTitle("Leaderboard")
                .WithFooter(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
                .WithCurrentTimestamp();

            using (var _dbContext = new BotContext())
            {
                var users = await _dbContext.Users.ToListAsync();
                users = users.OrderByDescending(p => p.Points).ToList();
                if(user != null) {
                    var index = users.FindIndex(p => p.Id == user.Id.ToString());
                    var _user = users.Find(p => p.Id == user.Id.ToString());
                    embed.WithDescription($"{index + 1}) {user.Mention} - {_user.Points} {currencyName}");
                } 
                else {
                    var _users = users.Take(10).ToList();
                    var sb = new StringBuilder();
                    for(int i = 0; i < _users.Count; i++) {
                        var id = ulong.Parse(_users[i].Id);
                        var _user = Context.Guild.GetUser(id);
                        sb.AppendLine($"{i + 1}) {_user.Mention} - {_users[i].Points} {currencyName}");
                    }
                    embed.WithDescription(sb.ToString());
                }
                
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}