using System;
using System.IO;
using System.Net;
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
                try
                {
                    var uri = new Uri(icon.Image);
                    var webClient = new WebClient();
                    webClient.DownloadFile(uri, filePath);
                }
                catch (Exception error)
                {
                    filePath = string.Empty;
                }

            return filePath;
        }
    }
}