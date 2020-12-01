using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ERIK.Bot.Configurations;
using ERIK.Bot.Models;

namespace ERIK.Bot.Extensions
{
    public static class IconExtension
    {

        public static string DownloadAndOrGet(this Icon icon, DiscordBotSettings botSettings, Guild guild)
        {
            var filePath = $"{botSettings.IconDirectory}/{guild.Id}-{icon.Id}";
            if (!File.Exists(filePath))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(icon.Image, filePath);
            }

            return filePath;
        }
    }
}
