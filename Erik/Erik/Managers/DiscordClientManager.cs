using Discord;
using Discord.WebSocket;

namespace Erik.Managers
{
    public class DiscordClientManager : BackgroundService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ILogger<DiscordClientManager> _logger;

        public DiscordClientManager(DiscordSocketClient discordSocketClient, ILogger<DiscordClientManager> logger)
        {
            _discordSocketClient = discordSocketClient;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _discordSocketClient.Log += async (msg) =>
            {
                await Task.CompletedTask;
                _logger.LogInformation(msg.ToString());
            };

            await _discordSocketClient.StartAsync();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
