using Discord;
using Discord.WebSocket;
using Erik.Configurations;
using Microsoft.Extensions.Options;

namespace Erik.Managers
{
    public class DiscordClientManager : BackgroundService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly BotConfiguration _botConfiguration;
        private readonly ILogger<DiscordClientManager> _logger;

        public DiscordClientManager(DiscordSocketClient discordSocketClient, ILogger<DiscordClientManager> logger, IOptions<BotConfiguration> botOptions)
        {
            _discordSocketClient = discordSocketClient;
            _logger = logger;
            _botConfiguration = botOptions.Value;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _discordSocketClient.Log += async (msg) =>
            {
                await Task.CompletedTask;
                _logger.LogInformation(msg.ToString());
            };
            await _discordSocketClient.LoginAsync(TokenType.Bot, _botConfiguration.Token);
            await _discordSocketClient.StartAsync();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
