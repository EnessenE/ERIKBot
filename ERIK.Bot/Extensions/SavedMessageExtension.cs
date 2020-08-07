using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERIK.Bot.Models.Reactions;
using Discord;
using Discord.Commands;

namespace ERIK.Bot.Extensions
{
    public static class SavedMessageExtension
    {
        public static Embed ToEmbed(this SavedMessage savedmsg)
        {
            //            var embedB = new EmbedBuilder
            //            {
            //                Title = savedmsg.Title,
            //                Description = savedmsg.Description
            //            };
            //            embed.AddField("Field title",
            //                "Field value. I also support [hyperlink markdown](https://example.com)!");
            //            embed.WithAuthor(Context.Client.CurrentUser);
            //            embedB.WithFooter(footer => footer.Text = "I am a footer.");
            //            embedB.WithColor(Color.Blue);
            //            embedB.WithTitle("I overwrote \"Hello world!\"");
            //            embedB.WithDescription("I am a description.");
            //            embedB.WithUrl("https://example.com");
            //            embedB.WithCurrentTimestamp();
            //            var embed = embedB.Build();

            //            return embed;
            return null;
        }
    }
}
