using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ERIK.Bot.Services
{
    public class StatusService: IHostedService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<StatusService> _logger;
        private DiscordBotSettings _options;
        private Thread _statusThread;
        private bool _stopped;

        public StatusService(ILogger<StatusService> logger, DiscordSocketClient client, IOptions<DiscordBotSettings> options)
        {
            _logger = logger;
            _client = client;
            _options = options.Value;
            _stopped = false;
        }


        public List<string> LoadJson()
        {
            var list = new List<string>();
            using (var r = new StreamReader("status.json")) //Move to appsettings
            {
                var json = r.ReadToEnd();
                var item = JsonConvert.DeserializeObject<RandomStatuses>(json);
                list = item.statuses;
            }

            return list;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting the status setter!");
            _statusThread = new Thread(StatusLogic);
            _statusThread.Name = "StatusThread";
            return Task.CompletedTask;
        }

        private void StatusLogic()
        {
            var randomStatus = LoadJson().PickRandom();

            Thread.Sleep(5000);
            while (!_stopped)
            {
                try
                {
                    _logger.LogInformation("Attempting to set the status");

                    _client.SetGameAsync(randomStatus);
                    _logger.LogInformation("Set the status to {msg}!", randomStatus);
                }
                catch (Exception error)
                {
                    _logger.LogError(error, "Failed to set status");
                }

                Thread.Sleep(_options.StatusInterval);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopped = true;
            _statusThread.Join();
            return Task.CompletedTask;
        }
    }
}