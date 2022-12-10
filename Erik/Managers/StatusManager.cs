using Discord;
using Discord.WebSocket;
using Erik.Configurations;
using Erik.Extensions;
using Microsoft.Extensions.Options;

namespace Erik.Managers
{
    public class StatusManager : IHostedService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly StatusConfiguration _statusSettings;
        private readonly ILogger<StatusManager> _logger;

        private Timer _timer;

        public StatusManager(DiscordSocketClient discordSocketClient, IOptions<StatusConfiguration> statusSettings, ILogger<StatusManager> logger)
        {
            _discordSocketClient = discordSocketClient;
            _logger = logger;
            _statusSettings = statusSettings.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(setStatus, null, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(60));
            _logger.LogInformation("Started status manager");
            return Task.CompletedTask;
        }

        private void setStatus(object? state)
        {
            _logger.LogInformation("Attempting to set a new status");
            var randomStatus = _statusSettings.Statuses.PickRandom();
            var newStatus = new Game(randomStatus, ActivityType.Playing);
            _discordSocketClient.SetActivityAsync(newStatus);
            _logger.LogInformation("Set new status {status}", randomStatus);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopped status manager");
            return Task.CompletedTask;
        }
    }
}
