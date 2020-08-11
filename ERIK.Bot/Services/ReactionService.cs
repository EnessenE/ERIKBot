﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using ERIK.Bot.Context;
using ERIK.Bot.Enums;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models.Reactions;
using Microsoft.Extensions.Logging;

namespace ERIK.Bot.Services
{
    public class ReactionService
    {
        private ILogger<ReactionService> _logger;
        private readonly EntityContext _context;

        public ReactionService(ILogger<ReactionService> logger, EntityContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task HandleReactionAsync(DiscordSocketClient client, Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = cachedMessage.Value;

            if (client.CurrentUser.Id == reaction.UserId)
            {
                return;
            }

            if (!cachedMessage.HasValue)
            {
                message = await cachedMessage.GetOrDownloadAsync();
            }

            _logger.LogInformation("A message was reacted on: {msg} by {author} with emoji {emoji}", message.Id, reaction.User.Value.Username, reaction.Emote.Name);

            var trackedMessage = await _context.GetMessageOnTrackIdAsync(message.Id);
            if (trackedMessage != null)
            {
                _logger.LogInformation("Tracking this message!");
                ReactionState state = reaction.Emote.Name.ToReactionState();
                if (state != ReactionState.Unknown)
                {
                    await ProcessMessage(client, message, trackedMessage, reaction, state);
                }
                else
                {
                    _logger.LogDebug("Invalid emoji");
                }
            }
        }

        public async Task ProcessMessage(DiscordSocketClient client, IUserMessage message, SavedMessage trackedMessage, SocketReaction reaction, ReactionState state)
        {
            switch (trackedMessage.Type)
            {
                case ReactionMessageType.LFG:
                    await ProcessLfg(client, message, trackedMessage, reaction, state);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task ProcessLfg(DiscordSocketClient client, IUserMessage message, SavedMessage trackedMessage, SocketReaction socketReaction, ReactionState state)
        {
            if (!trackedMessage.IsFinished)
            {
                //Clear previous user state (if any)
                _context.RemoveReaction(trackedMessage, socketReaction.UserId);

                if (trackedMessage.TotalJoined >= trackedMessage.JoinLimit && state == ReactionState.Joined)
                {
                    state = ReactionState.Alternate;
                }

                _context.AddReaction(trackedMessage, socketReaction.User.Value, state);

                foreach (var trackedId in trackedMessage.TrackedIds)
                {
                    try
                    {
                        var targetChannel = client.GetChannel(trackedId.ChannelId) as ITextChannel;
                        var targetMessage = await targetChannel.GetMessageAsync(trackedId.MessageId) as IUserMessage;
                        await targetMessage.ModifyAsync(m => { m.Embed = trackedMessage.ToEmbed(client); });
                    }
                    catch (Exception error)
                    {
                        _logger.LogError(error, "Failed editting a message.");
                    }
                }

                try
                {
                    //Clear new user reaction
                    await message.RemoveReactionAsync(socketReaction.Emote, socketReaction.User.Value);
                }
                catch (HttpException)
                {
                    var channel = message.Channel as IGuildChannel;
                    var owner = await channel.Guild.GetOwnerAsync();
                    var msgChannel = message.Channel as IMessageChannel;
                    await msgChannel.SendMessageAsync("<@" + owner.Id +
                                                      "> I require the MANAGE_REACTIONS permission for full functionality!");

                }
            }
        }
    }
}
