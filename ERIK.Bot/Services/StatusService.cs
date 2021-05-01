using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Discord.WebSocket;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ERIK.Bot.Services
{
    public class StatusService
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<StatusService> _logger;

        public StatusService(ILogger<StatusService> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        public void Start()
        {
            _logger.LogInformation("Starting the status setter!");
            new Thread(() =>
            {
                Thread.Sleep(5000);
                while (true)
                {
                    try
                    {
                        _logger.LogInformation("Attempting to set the status");

                        //We load the json in eachtime incase stuff has changed
                        //TODO: Migrate to appsettings or something that supports reload
                        var randomtext = LoadJson().PickRandom();
                        _client.SetGameAsync(randomtext);
                        _logger.LogInformation("Set the status to {msg}!", randomtext);
                    }
                    catch (Exception error)
                    {
                        _logger.LogError(error, "Failed to set status");
                    }

                    Thread.Sleep(900000);
                }
            }).Start();
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
    }
}