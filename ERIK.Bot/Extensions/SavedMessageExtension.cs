using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using ERIK.Bot.Models.Reactions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Enums;
using NJsonSchema.Validation.FormatValidators;

namespace ERIK.Bot.Extensions
{
    public static class SavedMessageExtension
    {
        public static Embed ToEmbed(this SavedMessage savedMsg, DiscordSocketClient client)
        {
            List<string> joined = new List<string>();
            List<string> alt = new List<string>();
            var embedB = new EmbedBuilder();
            embedB.Url = "https://time.is/UTC";
            embedB.WithFooter(footer => footer.Text = "Id: " + savedMsg.Id.ToString());
            embedB.WithColor(Color.Green);

            
            //author
            var author = client.GetUser(savedMsg.AuthorId);
            var authorBuilder = new EmbedAuthorBuilder { Name = author.Username, IconUrl = author.GetAvatarUrl() };

            embedB.Author = authorBuilder;

            embedB.AddField("Activity:", savedMsg.Title, true);
            embedB.AddField("Description:", savedMsg.Description, true);
            if (savedMsg.IsFinished)
            {
                embedB.AddField("Start Time:", savedMsg.Time, false);
            }
            else
            {
                embedB.AddField("Start Time:", savedMsg.Time, false);
            }

            if (!savedMsg.Published)
            {
                embedB.AddField("Publish time", savedMsg.PublishTime, true);
            }

            //embedB.WithDescription(savedMsg.Description);
            if (savedMsg.Reactions != null && savedMsg.Reactions.Count != 0)
            {
                foreach (var item in savedMsg.Reactions)
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

                var joinedMsg = "No one has joined yet";
                if (joined.Count > 0)
                {
                    joinedMsg = string.Empty;
                    foreach (var item in joined)
                    {
                        joinedMsg += item + ", ";
                    }
                    joinedMsg = joinedMsg.Remove(joinedMsg.Length - 2);
                }


                var altMsg = "There are no alternatives";
                if (alt.Count > 0)
                {
                    altMsg = string.Empty;
                    foreach (var item in alt)
                    {
                        altMsg += item + ", ";
                    }
                    altMsg = altMsg.Remove(altMsg.Length - 2);
                }

                embedB.AddField($"Joined ({savedMsg.TotalJoined}/{savedMsg.JoinLimit}):", joinedMsg, false);
                embedB.AddField($"Alternatives ({savedMsg.TotalAlternate}):", altMsg, true);
            }
            else
            {
                embedB.AddField("Joined:", "No one joined yet", false);
                embedB.AddField("Alternatives:", "There are no alternatives", true);
            }

            embedB.WithCurrentTimestamp();
            var embed = embedB.Build();

            return embed;
        }
    }
}
