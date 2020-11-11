using System;

namespace DiscordBotSample.Models {
    public class User {
        public string Id { get; set; }
        public int Points { get; set; }
        public DateTime LastDailyCollection { get; set; }
    }
}