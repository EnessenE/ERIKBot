using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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

        private async Task<dynamic> RandomCat()
        {
            string result = "https://cataas.com/cat";
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.thecatapi.com/v1/images/search?format=json&limit=3"))
                {
                    request.Headers.TryAddWithoutValidation("x-api-key", _catOptions.Token);

                    var response = await httpClient.SendAsync(request);
                    if (response != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var apiResult = JsonConvert.DeserializeObject<dynamic>(jsonString);
                        result = apiResult;
                    }

                }
            }

            return result;
        }

        public async Task MessageChannelCheck(SocketCommandContext context, SocketUserMessage message, SocketGuild guild)
        {
            if (context.Channel.Id == 284815121618829312) //#enesfw
            {               
                dynamic result = await RandomCat();

                for (int i = 0; i < 3; i++)
                {
                    string url = result[i].url;
                    _logger.LogInformation("Url to send: {url}", url);
                    await context.Channel.SendMessageAsync($"Look how cute! " + url);
                }
            }
        }
    }
}
