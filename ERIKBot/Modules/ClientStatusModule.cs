using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIKBot.Extensions;
using ERIKBot.Models;
using Newtonsoft.Json;

namespace ERIKBot.Modules
{
    public class ClientStatusModule //: ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public ClientStatusModule(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task Start()
        {

            Console.WriteLine("Starting the status setter!");
            new Thread(() =>
            {
                Thread.Sleep(5000);
                while (true)
                {
                    try
                    {
                        Console.WriteLine("Attempting to set the status");

                        string randomtext = LoadJson().PickRandom();
                        _client.SetGameAsync(randomtext);
                        Console.WriteLine("Set the status!");

                    }
                    catch (Exception error)
                    {
                        Console.WriteLine("Failed to set status");
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
