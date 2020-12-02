using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ERIK.Bot.Services
{
    public class StatusService
    {
        private readonly ILogger<StatusService> _logger;
        private readonly DiscordSocketClient _client;

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

                        string randomtext = LoadJson().PickRandom();
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
            List<string> list = new List<string>();
            using (StreamReader r = new StreamReader("status.json")) //Move to appsettings
            {
                string json = r.ReadToEnd();
                var item = JsonConvert.DeserializeObject<RandomStatuses>(json);
                list = item.statuses;
            }

            return list;
        }
    }
}
