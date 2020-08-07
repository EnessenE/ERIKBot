using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
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
            //Clear previous user state (if any)
            _context.RemoveReaction(trackedMessage, socketReaction.UserId);
            _context.AddReaction(trackedMessage, socketReaction.User.Value, state);

            string content = "Reacted:";
            foreach (var reaction in trackedMessage.Reactions)
            {
                var user = client.GetUser(reaction.User.Id);
                if (reaction.State != ReactionState.Left)
                {
                    content += reaction.State.ToString() + ": " + user.Username + ", ";
                }
            }
            await message.ModifyAsync(m => { m.Content = content; });

            //Clear new user reaction
            await message.RemoveReactionAsync(socketReaction.Emote, socketReaction.User.Value);
        }
    }
}
