using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Discord.Audio;
using Microsoft.Extensions.Options;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using ERIK.Bot.Models.Reactions;
using ERIK.Bot.Services;
using Microsoft.Extensions.Logging;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace ERIK.Bot.Modules
{
    public class ApexModule : InteractiveBase
    {
        private readonly CommandService _commandService;
        private readonly Responses _responses;
        private EntityContext _context;
        private readonly CatContext _catContext;
        private readonly ILogger<MiscModule> _logger;
        private AudioService _audioService;


        public ApexModule(CommandService commandService, IOptions<Responses> responses, EntityContext context, ILogger<MiscModule> logger, CatContext catContext, AudioService audioService)
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
            await ReplyAsync(_responses.ApexSentence.PickRandom() + " **" + _responses.ApexCharacters.PickRandom() + "**?");
        }
    }
}
