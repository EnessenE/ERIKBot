using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;

namespace ERIK.Bot.Handlers
{

    public static class EmbedHandler
    {

        /* This file is where we can store all the Embed Helper Tasks (So to speak). 
             We wrap all the creations of new EmbedBuilder's in a Task.Run to allow us to stick with Async calls. 
             All the Tasks here are also static which means we can call them from anywhere in our program. */
        public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithFooter(version)
                .WithCurrentTimestamp().Build()));
            return embed;
        }

        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"ERROR OCCURRED FROM - {source}")
                .WithDescription($"**Error Details**: \n{error}")
                .WithColor(Color.DarkRed)
                .WithFooter(version);
                .WithCurrentTimestamp().Build());
            return embed;
        }

    }
}
