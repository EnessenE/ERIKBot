using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERIK.Bot.Configurations;
using ERIK.Bot.Models.Cats;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace ERIK.Bot.Context
{
    public class CatContext
    {
        private readonly CatAPI _catOptions;
        private readonly ILogger<CatContext> _logger;

        public CatContext(ILogger<CatContext> logger, IOptions<CatAPI> catOptionsData)
        {
            _catOptions = catOptionsData.Value;
            _logger = logger;
        }

        public async Task<List<Cat>> RandomCats(int amount)
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
                _logger.LogError("Failed cat call", error);
            }

            return null;
        }
    }
}