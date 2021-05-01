using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ERIK.Bot.Context;
using Microsoft.Extensions.Logging;

namespace ERIK.Bot.Services
{
    public class ReactionService
    {
        private readonly EntityContext _context;
        private readonly ILogger<ReactionService> _logger;

        public ReactionService(ILogger<ReactionService> logger, EntityContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task HandleReactionAsync(DiscordSocketClient client, Cacheable<IUserMessage, ulong> cachedMessage,
            ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = cachedMessage.Value;

            if (client.CurrentUser.Id == reaction.UserId) return;

            if (!cachedMessage.HasValue) message = await cachedMessage.GetOrDownloadAsync();

            _logger.LogDebug("A message was reacted on: {msg} by {author} with emoji {emoji}", message.Id,
                reaction.User.Value.Username, reaction.Emote.Name);
        }
    }
}