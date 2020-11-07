using System;
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

            
        }

        public async Task ProcessMessage(DiscordSocketClient client, IUserMessage message, SocketReaction reaction, ReactionState state)
        {
           
        }

    }
}
