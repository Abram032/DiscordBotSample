using System.Collections.Generic;
using DiscordBotSample.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotSample.Data {
    public class BotContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=user.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(p => p.Id);
        }
    }
} 