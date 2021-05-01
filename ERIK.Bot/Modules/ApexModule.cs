using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class ApexModule : InteractiveBase
    {
        private readonly CatContext _catContext;
        private readonly CommandService _commandService;
        private readonly ILogger<MiscModule> _logger;
        private readonly Responses _responses;
        private AudioService _audioService;
        private EntityContext _context;


        public ApexModule(CommandService commandService, IOptions<Responses> responses, EntityContext context,
            ILogger<MiscModule> logger, CatContext catContext, AudioService audioService)
        {
            _commandService = commandService;
            _context = context;
            _responses = responses.Value;
            _logger = logger;
            _catContext = catContext;
            _audioService = audioService;
        }

        [Command("apex")]
        [Summary("What apex character should I play?")]
        public async Task Apex()
        {
            await ReplyAsync(_responses.ApexSentence.PickRandom() + " **" + _responses.ApexCharacters.PickRandom() +
                             "**?");
        }
    }
}