using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using ERIK.Bot.Models.Reactions;
using ERIK.Bot.Modules;
using Microsoft.Extensions.Logging;

namespace ERIK.Bot.Services
{
    public class LfgService
    {
        private readonly ILogger<LfgService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly EntityContext _context;
        private readonly LfgModule _lfgModule;

        public LfgService(ILogger<LfgService> logger, DiscordSocketClient client, EntityContext context, LfgModule lfgModule)
        {
            _logger = logger;
            _client = client;
            _context = context;
            _lfgModule = lfgModule;
        }

        public Task Start()
        {
            _logger.LogInformation("Starting the status setter!");
            new Thread(() =>
            {
                Thread.Sleep(6000);
                while (true)
                {
                    _logger.LogInformation("Checking for LFG's");
                    CheckForPublishPosts().ConfigureAwait(false);
                    CheckForNotification().ConfigureAwait(false);
                    CheckForFinished().ConfigureAwait(false);
                    Thread.Sleep(60000);
                }
            }).Start();
            return Task.CompletedTask;
        }

        private async Task CheckForFinished()
        {
            _logger.LogInformation("Processing Finished LFGs");

            var listOfPublished = await _context.GetAllNonFinished(true);

            foreach (var message in listOfPublished)
            {

                if (message.Time != null)
                {
                    var utcNow = DateTime.Now.ToUniversalTime();
                    var utcFinalTime = message.Time.ToUniversalTime();
                    if (utcNow >= utcFinalTime)
                    {
                        message.IsFinished = true;
                        foreach (var trackedMessage in message.TrackedIds)
                        {
                            try
                            {
                                var channel = _client.GetChannel(trackedMessage.ChannelId) as ITextChannel;
                                var sentMessage = await channel.GetMessageAsync(trackedMessage.MessageId) as IUserMessage;
                                _ = sentMessage.RemoveAllReactionsAsync().ConfigureAwait(false);
                                await sentMessage.ModifyAsync(m =>
                                {
                                    m.Embed = message.ToEmbed(_client);
                                    m.Content = message.AllJoined.ToUserList();
                                });
                                _logger.LogInformation("Finished one post.");
                            }
                            catch (Exception error)
                            {
                                _logger.LogError(error, "Failed 'fixing' one message.");
                            }
                        }
                    }
                    _context.Update(message);
                }
            }
            _context.SaveChanges();
        }

        private async Task CheckForNotification()
        {
            _logger.LogInformation("Processing Notifications");
            var listOfPublished = await _context.GetAllPublishedAndNonFinished(false);
            var listToNotify = new List<SavedMessage>();

            foreach (var message in listOfPublished)
            {
                if (message.PublishTime != null)
                {
                    var utcNow = DateTime.Now.ToUniversalTime();
                    var utcFinalTime = message.Time.ToUniversalTime();
                    if (utcFinalTime.AddMinutes(-15) <= utcNow)
                    {
                        listToNotify.Add(message);
                    }
                }
            }

            await NotifyUsers(listToNotify);
        }

        private Task NotifyUsers(List<SavedMessage> messages)
        {
            _logger.LogInformation("Notifying users.");
            foreach (var message in messages)
            {
                try
                {
                    foreach (var reaction in message.Reactions)
                    {
                        if (reaction.HasJoined)
                        {
                            var user = _client.GetUser(reaction.User.Id); 
                            _logger.LogInformation("Notifying {user}.", user.Username);

                            var guild = _client.GetGuild(message.GuildId);
                            _ = user.SendMessageAsync(
                                    $"***Alert*** \n Prepare to synchronize in Orbit for a LFG you have signed up for. You will be playing the activity called *{message.Title}*. Synchronize in the *{guild.Name}* guild. \n Activity starting at {message.Time:HH:mm:ss}. The following users have joined: " + message.AllJoined.ToUserList())
                                .ConfigureAwait(false);
                        }
                    }

                    message.Notified = true;
                    _context.Update(message);
                    _context.SaveChanges();
                }
                catch (Exception error)
                {
                    _logger.LogError(error, "Failed sending the notification for LFG {guid}", message.Id);
                }
            }

            return Task.CompletedTask;
        }

        private async Task CheckForPublishPosts()
        {
            _logger.LogInformation("Processing Publishposts");
            var listOfAllNonPublished = await _context.GetAllNonPublished();
            var listToPublish = new List<SavedMessage>();
            foreach (var message in listOfAllNonPublished)
            {
                if (message.PublishTime != null)
                {
                    var utcNow = DateTime.Now.ToUniversalTime();
                    var utcPublish = message.PublishTime.ToUniversalTime();
                    if (utcPublish <= utcNow)
                    {
                        listToPublish.Add(message);
                        _logger.LogInformation("Preparing to publish {id}", message.Id);
                    }
                }
            }

            _ = PublishPost(listToPublish).ConfigureAwait(false);
        }

        private async Task PublishPost(List<SavedMessage> messages)
        {
            _logger.LogInformation("Publishing posts");
            if (messages != null && messages.Count > 0)
            {
                foreach (var message in messages)
                {
                    try
                    {
                        Guild guild = null;
                        IGuildUser guildOwner = null;
                        ITextChannel prePublishChannel = null;

                        if (message.GuildId > 0)
                        {
                            guild = _context.GetOrCreateGuild(message.GuildId);
                            IGuild discordGuild = _client.GetGuild(message.GuildId);
                            guildOwner = await discordGuild.GetOwnerAsync();
                            prePublishChannel = _client.GetChannel(guild.LfgPrepublishChannelId) as ITextChannel;
                        }

                        if (guild != null && guild.LfgPublishChannelId > 0)
                        {
                            message.Published = true;

                            var channel = _client.GetChannel(guild.LfgPublishChannelId) as ITextChannel;
                            IUserMessage sentMessage = await channel.SendMessageAsync(embed: message.ToEmbed(_client));
                            message.TrackedIds.Add(new TrackedMessage()
                            {
                                ChannelId = sentMessage.Channel.Id,
                                MessageId = sentMessage.Id
                            });

                            await _lfgModule.ConnectMessage(message, sentMessage);
                            _context.UpdateRange(messages);
                            _context.SaveChanges();
                            await prePublishChannel.SendMessageAsync(
                                $"I successfully published the LFG with id {message.Id} for {message.Title}.");
                            _logger.LogInformation("I successfully published the LFG with id {Id} for {Title}.", message.Id, message.Title);

                        }
                        else
                        {
                            await guildOwner.SendMessageAsync(
                                $"I couldn't find the selected publish channel. This caused me to fail publishing the LFG with id {message.Id} for {message.Title}.");
                            _logger.LogInformation("I couldn't find the selected publish channel. This caused me to fail publishing the LFG with id {Id} for {Title}.", message.Id, message.Title);

                        }
                    }
                    catch (Exception error)
                    {
                        _logger.LogError(error, "Failed sending the publish for LFG {guid}", message.Id);
                    }
                }
            }
        }
    }
}
