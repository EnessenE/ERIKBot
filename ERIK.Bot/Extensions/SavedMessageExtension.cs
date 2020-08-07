using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERIK.Bot.Models.Reactions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Enums;

namespace ERIK.Bot.Extensions
{
    public static class SavedMessageExtension
    {
        public static Embed ToEmbed(this SavedMessage savedmsg, DiscordSocketClient client)
        {
            List<string> joined = new List<string>();
            List<string> alt = new List<string>();
            var embedB = new EmbedBuilder
            {
                Title = savedmsg.Title,
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
                    var user = client.GetUser(item.User.Id);
                    switch (item.State)
                    {
                        case ReactionState.Joined:
                        {
                            joined.Add(user.Username);
                            break;
                        }
                        case ReactionState.Alternate:
                        {
                            alt.Add(user.Username);
                            break;
                        }
                    }
                }

                var joinedMsg = "no one joined yet";
                if (joined.Count > 0)
                {
                    joinedMsg = string.Empty;
                    foreach (var item in joined)
                    {
                        joinedMsg += item + ", ";
                    }
                    joinedMsg.Remove(joinedMsg.Length - 2);
                }


                var altMsg = "there are no alternatives";
                if (alt.Count > 0)
                {
                    altMsg = string.Empty;
                    foreach (var item in alt)
                    {
                        altMsg += item + ", ";
                    }
                    altMsg.Remove(altMsg.Length - 2);
                }

                embedB.AddField("Joined:", joinedMsg, true);
                embedB.AddField("Alternatives:", altMsg, true);
            }
            else
            {
                embedB.AddField("Joined:", "no one joined yet", true);
                embedB.AddField("Alternatives:", "there are no alternatives", true);
            }

            embedB.WithCurrentTimestamp();
            var embed = embedB.Build();

            return embed;
        }
    }
}
