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
    public class StatusModule : InteractiveBase
    {
        private readonly ILogger<MiscModule> _logger;



        public StatusModule(ILogger<MiscModule> logger)
        {
            _logger = logger;
        }

        //[Command("test", RunMode = RunMode.Async)]
        [Summary("")]
        public async Task Help()
        {

        }


    }
}
