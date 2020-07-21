using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ERIK.Bot.Modules
{
    public class ClientStatusModule //: ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private ILogger<ClientStatusModule> _logger;

        public ClientStatusModule(DiscordSocketClient client, ILogger<ClientStatusModule> logger)
        {
            _client = client;
            _logger = logger;
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
                        _logger.LogWarning("Failed to set status");
                    }
                    Thread.Sleep(900000);

                }
            }).Start();
        }

        public List<string> LoadJson()
        {
            List<string> list = new List<string>();
            using (StreamReader r = new StreamReader("status.json"))
            {
                string json = r.ReadToEnd();
                var item = JsonConvert.DeserializeObject<RandomStatuses>(json);
                list = item.statuses;
            }

            return list;
        }
    }
}
