﻿using System;

namespace ERIK.Bot.Configurations
{
    public class DiscordBotSettings
    {
        public string Token { get; set; }
        public string IconDirectory { get; set; }
        public string LavaKey { get; set; }
        public string LavaHost { get; set; }
        public string ApexToken { get; set; }

        public TimeSpan StatusInterval { get; set; }
    }
}