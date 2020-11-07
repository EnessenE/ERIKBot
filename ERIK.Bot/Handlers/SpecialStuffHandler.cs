

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Models.Cat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace ERIK.Bot.Modules
{
    public class SpecialStuffHandler
    {
        private readonly ILogger<SpecialStuffHandler> _logger;
        private CatAPI _catOptions;

        public SpecialStuffHandler(ILogger<SpecialStuffHandler> logger, IOptions<CatAPI> catOptionsData)
        {
            _catOptions = catOptionsData.Value;
            _logger = logger;
        }

        private async Task<List<Cat>> RandomCats(int amount)
        {
            try
            {
                var client = new RestClient("https://api.thecatapi.com/v1");
                client.AddDefaultHeader("x-api-key", _catOptions.Token);
                var request = new RestRequest("images/search?format=json", DataFormat.Json);
                request.AddParameter("limit", amount);

                var response = client.Get(request);

                var x = JsonConvert.DeserializeObject<List<Cat>>(response.Content);
                return x;
            }
            catch (Exception error)
            {
                _logger.LogError("Failed cat call",error);
            }

            return null;
        }

        public async Task MessageChannelCheck(SocketCommandContext context, SocketUserMessage message, SocketGuild guild)
        {
            //string result = "https://cataas.com/cat";
            if (context.Channel.Id == 284815121618829312) //#enesfw
            {               
                var result = await RandomCats(3);

                if (result != null)
                {
                    foreach (var cat in result)
                    {
                        string url = cat.url;
                        _logger.LogInformation("Url to send: {url}", url);
                        await context.Channel.SendMessageAsync($"Look how cute! " + url);
                    }
                }
                else
                {
                    string url = "https://cataas.com/cat"; _logger.LogInformation("Url to send: {url}", url);
                    await context.Channel.SendMessageAsync($"Look how adorable! " + url);
                    _logger.LogInformation("FALLBACK! Url to send: {url}", url);
                    await context.Channel.SendMessageAsync($"Look how cute! " + url);
                }
            }
        }
    }
}
