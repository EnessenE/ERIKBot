using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ERIK.Bot.Context;
using ERIK.Bot.Enums;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using ERIK.Bot.Models.Reactions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;

namespace ERIK.Bot.Modules
{
    public class LfgModule : InteractiveBase
    {
        private readonly EntityContext _context;
        private readonly ILogger<LfgModule> _logger;

        public LfgModule(EntityContext context, ILogger<LfgModule> logger)
        {
            _context = context;
            _logger = logger;
        }


        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg create", RunMode = RunMode.Async)]
        [Summary("Create a LFG with Activity, Description, Start time and setup a time to (automatically) publish the lfg")]
        public async Task CreateLfg()
        {

            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);

            if (guild.LfgPrepublishChannelId < 1)
            {
                await ReplyAsync("This command requires the pre-publish channel to be set.");
                return;
            }

            var prePublish = this.Context.Guild.GetChannel(guild.LfgPrepublishChannelId) as ITextChannel;

            if (prePublish != null)
            {
                await ReplyAsync("Couldn't find the targetted pre-publish channel");
                return;
            }

            DateTime publishtime;
            var origMessage = await ReplyAsync("Preparing LFG creation.");
            var title = await AskForItem<string>(origMessage, "What is the activity?");
            var desc = await AskForItem<string>(origMessage, "Give me a description!");
            var startTime = await AskForDate("raid");
            var response = await AskForItem<string>(origMessage, $"Do you want to publish this automatically? Y/N");
            if (response.ToLower() != "y")
            {
                await ReplyAsync("Creating lfg!");
                publishtime = default;
            }
            else
            {
                publishtime = await AskForDate("publish");
            }

            SavedMessage savedMessage = new SavedMessage
            {
                IsFinished = false,
                AuthorId = this.Context.Message.Author.Id,
                Type = ReactionMessageType.LFG,
                Time = startTime,
                Title = Convert.ToString(title),
                Description = Convert.ToString(desc),
                PublishTime = publishtime,
                GuildId = this.Context.Guild.Id,
                JoinLimit = 6
            };

            _context.CreateMessage(savedMessage);

            IUserMessage msg = await prePublish.SendMessageAsync(embed: savedMessage.ToEmbed(this.Context.Client));

            _context.Update(savedMessage);
            _context.SaveChanges();
            await origMessage.DeleteAsync();

            await ConnectMessage(savedMessage, msg);

        }

        //the string parameter is to indicate whether the question is related to the raid or the publishing of the lfg
        public async Task<DateTime> AskForDate(string s)
        {

            var origMessage = await ReplyAsync("Asking for time.");
            DateTime finalDateTime = DateTime.Today;

            var dayResult = await AskForItem<DateTime>(origMessage, $"Tell me when the {s} is taking place in DD/MM/YY (CEST)");
            var timeResult = await AskForItem<DateTime>(origMessage, $"And the time? HH:MM (CEST)");

            await origMessage.DeleteAsync();


            dayResult = dayResult.AddHours(timeResult.Hour);
            dayResult = dayResult.AddMinutes(timeResult.Minute);

            return dayResult;
        }



        public async Task<T> AskForItem<T>(IUserMessage originalMessage, string text)
        {
            T result;
            bool firstFail = false;
            while (true)
            {
                await originalMessage.ModifyAsync(m => { m.Content = text; });
                var item = await NextMessageAsync();
                if (item != null)
                {
                    try
                    {
                        var x = item.ToString();
                        result = (T)Convert.ChangeType(x, typeof(T));
                        if (result != null)
                        {
                            await item.DeleteAsync();
                            break;
                        }
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Failed", error);
                        if (!firstFail)
                        {
                            firstFail = true;
                            await originalMessage.ModifyAsync(m =>
                            {
                                m.Content = text + "\n Failed the conversion for your input. Try again please.";
                            });
                        }
                    }
                }
            }

            return result;
        }

        public async Task<T> AskForItem<T>(string text)
        {
            T result;
            IUserMessage sentMessage;
            bool firstFail = false;
            while (true)
            {
                sentMessage = await ReplyAsync(text);
                var item = await NextMessageAsync();
                if (item != null)
                {
                    try
                    {
                        var x = item.ToString();
                        result = (T)Convert.ChangeType(x, typeof(T));
                        if (result != null)
                        {
                            await item.DeleteAsync();
                            break;
                        }
                    }
                    catch (Exception error)
                    {
                        _logger.LogError("Failed", error);
                        if (!firstFail)
                        {
                            firstFail = true;
                            await sentMessage.ModifyAsync(m =>
                            {
                                m.Content = text + "\n Failed the conversion for your input. Try again please.";
                            });
                        }
                    }
                }
            }

            return result;
        }

        [Command("lfg id")]
        [Summary("Retrieve an lfg by id number")]
        public async Task GetLfgById(int lfgid)
        {
            SavedMessage savedMessage;
            if (lfgid != 0)
            {
                savedMessage = _context.GetSavedMessageById(lfgid);
                if (savedMessage != null)
                {
                    if (savedMessage.Published)
                    {
                        var sentMessage = await ReplyAsync(embed: savedMessage.ToEmbed(this.Context.Client));
                        await ConnectMessage(savedMessage, sentMessage);
                    }
                    else
                    {
                        await ReplyAsync("Sorry, this LFG hasn't been published yet.");
                    }
                }
                else
                {
                    await ReplyAsync("No valid lfg id found");
                }
            }
            else
            {
                await ReplyAsync("No valid lfg id found");
            }

        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg prepublish")]
        [Summary("Before publishing an LFG you can pre-create it in a channel")]
        public async Task SetChannelPrePublish(IChannel channel)
        {
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
            guild.LfgPrepublishChannelId = channel.Id;
            _context.SaveChanges();
            await ReplyAsync($"Set the new pre-publish channel to <@{channel.Id}>");
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("lfg delete")]
        [Summary("Delete an LFG. Usage: !lfg delete [id]")]
        public async Task DeleteLFG(int id)
        {
            SavedMessage message = _context.GetMessage(id);
            if (message != null)
            {
                if (message.GuildId == this.Context.Guild.Id)
                {
                    foreach (var trackedMessage in message.TrackedIds)
                    {
                        var channel = this.Context.Guild.GetChannel(trackedMessage.ChannelId) as ITextChannel;
                        var foundMessage = await channel.GetMessageAsync(trackedMessage.MessageId) as IUserMessage;
                        await foundMessage.ModifyAsync(m =>
                        {
                            m.Embed = null;
                            m.Content = $"This LFG({id}) was deleted by {this.Context.Message.Author.Username}";
                        });
                    }

                    _context.Remove(message);
                    _context.SaveChanges();
                }
                else
                {
                    await ReplyAsync("This LFG wasn't created in this guild and can't be deleted because of it.");
                }
            }
            else
            {
                await ReplyAsync("Couldn't find the targetted LFG.");
            }
        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg normal")]
        [Summary("Sets the publish channel of the bot")]
        public async Task SetChannelPublish(IChannel channel)
        {
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
            guild.LfgPublishChannelId = channel.Id;
            _context.SaveChanges();
            await ReplyAsync($"Set the new publish channel to <@{channel.Id}>");
        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg publish")]
        [Summary("Publish all current pre-published LFGs to the pre-selected publish channel")]
        public async Task Publish()
        {
            List<SavedMessage> messages = await _context.GetAllNonPublished(this.Context.Guild.Id);
            var targetChannel = _context.GetOrCreateGuild(this.Context.Guild.Id).LfgPublishChannelId;
            if (messages != null && messages.Count > 0)
            {
                if (targetChannel > 0)
                {
                    var channel = this.Context.Client.GetChannel(targetChannel) as IMessageChannel;
                    foreach (var message in messages)
                    {
                        message.Published = true;
                        if (channel != null)
                        {
                            IUserMessage sentMessage = await channel.SendMessageAsync(embed: message.ToEmbed(this.Context.Client));

                            await ConnectMessage(message, sentMessage);
                        }
                        else
                        {
                            await ReplyAsync("I couldn't find the target channel. Try again");
                        }
                    }
                    await ReplyAsync("I successfully published all non published LFG posts for this guild.");
                }
                else
                {
                    await ReplyAsync("No publish channel set.");
                }
            }
            else
            {
                await ReplyAsync("No posts to publish.");
            }

        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg settings")]
        [Summary("Settings of the LFG section")]
        public async Task Settings()
        {
            var guild = _context.GetOrCreateGuild(this.Context.Guild.Id);

            var embedBuilder = new EmbedBuilder
            {
                Title = "Guild LFG settings",
                Description = "Settings used by the bot."
            };

            var prePublishChannel = this.Context.Guild.GetChannel(guild.LfgPrepublishChannelId) as IGuildChannel;
            if (prePublishChannel != null)
            {
                embedBuilder.AddField("Pre publish channel", prePublishChannel);
            }
            else
            {
                embedBuilder.AddField("Pre publish channel", "None set");
            }

            var publishChannel = this.Context.Guild.GetChannel(guild.LfgPublishChannelId);
            if (publishChannel != null)
            {
                embedBuilder.AddField("Publish channel", prePublishChannel);
            }
            else
            {
                embedBuilder.AddField("Publish channel", "None set");
            }

            embedBuilder.WithColor(Color.Blue);
            await ReplyAsync(embed: embedBuilder.Build());
        }


        /// <summary>
        /// Automaticly add emojis and start tracking the sent message for changes
        /// </summary>
        /// <param name="message">The saved (LFG) message</param>
        /// <param name="sentMessage">The sent discord message</param>
        /// <returns></returns>
        public async Task ConnectMessage(SavedMessage message, IUserMessage sentMessage)
        {
            var checkMark = new Emoji("✔️");
            var cross = new Emoji("❌");
            var question = new Emoji("❓");
            var reactions = new IEmote[] { checkMark, cross, question };

            if (message.TrackedIds == null)
            {
                message.TrackedIds = new List<TrackedMessage>();
            }

            message.TrackedIds.Add(new TrackedMessage()
            {
                ChannelId = sentMessage.Channel.Id,
                MessageId = sentMessage.Id
            });

            _context.Update(message);
            _context.SaveChanges();

            await sentMessage.AddReactionsAsync(reactions); //One call saves data and doesn't hit the API rate limiting
        }

    }
}


// Reference (old)Code
//[RequireUserPermission(GuildPermission.MentionEveryone)]
//[Command("lfg create", RunMode = RunMode.Async)]
//[Summary("Save the last [amount] messages in the selected text channel. Usage: !save [email] [amount (default 100)]")]
//public async Task CreateLfg(string activity, string desc, DateTime time)
//{
//    Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);

//    if (this.Context.Channel.Id != guild.LfgPrepublishChannelId)
//    {
//        await ReplyAsync("This command can only be used in the pre-publish channel");
//        return;
//    }
//    SavedMessage savedMessage = new SavedMessage
//    {
//        IsFinished = false,
//        AuthorId = this.Context.Message.Author.Id,
//        Type = ReactionMessageType.LFG,
//        Time = time,
//        Title = activity,
//        Description = desc,
//        GuildId = this.Context.Guild.Id
//    };

//    IUserMessage msg = await ReplyAsync(embed: savedMessage.ToEmbed(this.Context.Client));

//    savedMessage.TrackedIds = new List<TrackedMessage>();
//    savedMessage.TrackedIds.Add(new TrackedMessage() { ChannelId = msg.Channel.Id, MessageId = msg.Id });

//    _context.CreateMessage(savedMessage);
//    await ConnectMessage(savedMessage, msg);

//}

//public async Task<DateTime> AskForDate()
//{
//    DateTime finalDateTime = DateTime.Today;
//    string dayText = "What day?\n";
//    for (int i = 0; i < 7; i++)
//    {
//        dayText += $"{i} - {DateTime.Now.AddDays(i):D}\n";
//    }

//    var dayResult = await AskForItem<int>(dayText);

//    string timeText = "What time? HH:mm";
//    DateTime time = await AskForItem<DateTime>(timeText);

//    finalDateTime.AddDays(dayResult);
//    finalDateTime.AddHours(time.Hour);
//    finalDateTime.AddMinutes(time.Minute);

//    return finalDateTime;

//}
