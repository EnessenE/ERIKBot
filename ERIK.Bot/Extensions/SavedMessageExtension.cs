using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERIK.Bot.Models.Reactions;
using Discord;
using Discord.Commands;
using ERIK.Bot.Enums;

namespace ERIK.Bot.Extensions
{
    public static class SavedMessageExtension
    {
        public static Embed ToEmbed(this SavedMessage savedmsg)
        {
            List<DiscordUser> joined = new List<DiscordUser>();
            List<DiscordUser> alt = new List<DiscordUser>();
            var embedB = new EmbedBuilder{
                Description = savedmsg.Description
            };

            embedB.WithFooter(footer => footer.Text = "Kga lfg");
            embedB.WithColor(Color.Green);

            embedB.AddField("Activity:", savedmsg.Title, true);
            embedB.AddField("Start Time:", savedmsg.Time, true);
            embedB.AddField("ID", "No implementation", true);

            embedB.WithDescription(savedmsg.Description);
            if (savedmsg.Reactions != null && savedmsg.Reactions.Count != 0)
            {
                foreach (var item in savedmsg.Reactions)
                {
                    if (item.State.Equals(ReactionState.Joined)) { joined.Add(item.User); }
                    else { alt.Add(item.User); }
                }

                embedB.AddField("Joined:", joined, true);
                embedB.AddField("Alternatives:", alt, true);
            }
            else
            {
                embedB.AddField("Joined:", "no one joined up yet", true);
                embedB.AddField("Alternatives:", "there are no alternatives", true);
            }

            embedB.WithCurrentTimestamp();
            var embed = embedB.Build();

            return embed;
        }
    }
}
