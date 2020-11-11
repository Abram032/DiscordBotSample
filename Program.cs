using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotSample.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBotSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            PrepareDatabaseAsync(host).Wait();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration(configurationBuilder => {
                    configurationBuilder.AddJsonFile("config.json");
                });

        public static async Task PrepareDatabaseAsync(IHost host)
        {
            using(var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<BotContext>();
                if(await context.Database.GetService<IRelationalDatabaseCreator>().ExistsAsync() == false)
                {
                    await context.Database.MigrateAsync();
                }
                else
                {
                    var migrations = await context.Database.GetPendingMigrationsAsync();
                    if(migrations.ToList().Count > 0)
                    {
                        await context.Database.MigrateAsync();
                    }
                }
            }
        }
    }
}
