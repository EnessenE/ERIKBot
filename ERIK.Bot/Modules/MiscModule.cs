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
    public class MiscModule : InteractiveBase
    {
        private readonly CommandService _commandService;
        private readonly Responses _responses;
        private EntityContext _context;
        private readonly CatContext _catContext;
        private readonly ILogger<MiscModule> _logger;
        private AudioService _audioService;


        public MiscModule(CommandService commandService, IOptions<Responses> responses, EntityContext context, ILogger<MiscModule> logger, CatContext catContext, AudioService audioService)
        {
            _commandService = commandService;
            _context = context;
            _responses = responses.Value;
            _logger = logger;
            _catContext = catContext;
            _audioService = audioService;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Summary("A summary of all available commands")]
        public async Task Help()
        {
            var prefix = _context.GetOrCreateGuild(this.Context.Guild.Id).Prefix;

            List<CommandInfo> commands = _commandService.Commands.ToList();
            string message = "";

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string descriptionText = String.Empty;

                descriptionText += command.Summary ?? "No description available";

                message += ($"{prefix}{command.Name}            {descriptionText}\n");
            }

            //replyandeleteasync is broken atm
            var tempMsg = await ReplyAsync("Sent you a PM with all commands!");
            this.Context.User.SendMessageAsync("Here's a list of commands and their description: ```" + message + "```", false);
            this.Context.Message.DeleteAsync();
            Thread.Sleep(TimeSpan.FromSeconds(5));
            tempMsg.DeleteAsync();
        }

        [Command("ping")]
        [Summary("Send a response")]
        public async Task Pong()
        {
            await ReplyAsync(_responses.Pong.PickRandom());
        }


        [Command("dab")]
        [Summary("Dabs")]
        public async Task Dab()
        {
            await ReplyAsync("*dabs*");
        }

        [Command("martijn")]
        [Summary("Gives details about martijn")]
        public async Task Martijn()
        {
            await ReplyAsync(_responses.Martijn.PickRandom());
        }

        [Command("apex")]
        [Summary("What apex character should I play?")]
        public async Task Apex()
        {
            await ReplyAsync(_responses.ApexSentence.PickRandom() + " **" + _responses.ApexCharacters.PickRandom() + "**?");
        }

        [Command("prefix")]
        [Summary("Set the prefix")]
        public async Task Prefix(string newPrefix)
        {
            string response = string.Empty;
            string oldPrefix = "ERR";
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);

            oldPrefix = guild.Prefix;
            guild.Prefix = newPrefix;

            _context.Update(guild);
            _context.SaveChanges();

            response = $"Changed this guilds prefix from {oldPrefix} to {guild.Prefix}";

            await ReplyAsync(response);
        }

        [Command("cat")]
        [Summary("returns a cat")]
        public async Task Cat()
        {
            var result = await _catContext.RandomCats(1);

            if (result != null)
            {
                foreach (var cat in result)
                {
                    _logger.LogDebug("Url to send: {url}", cat.url);
                    await ReplyAsync($"Look how cute! " + cat.url);
                }
            }
            else
            {
                ReplyAsync("Couldn't find any cats :(");
            }
        }

        [Command("serverinfo")]
        [Summary("Shows some guild information that was retreived from discord.")]
        public async Task Serverinfo()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var guild = this.Context.Guild;
            var embed = new EmbedBuilder();
            embed.WithThumbnailUrl(guild.IconUrl);
            embed.WithTitle($"Server info {guild.Name}");
            embed.WithDescription(guild.Description);
            embed.WithFooter(version);
            var stats = string.Empty;
            stats += $"**Id:** {guild.Id}\n";
            stats += $"**Total members:** {guild.MemberCount}\n";
            stats += $"**Owner:** {guild.Owner.Nickname}\n";
            stats += $"**Region:** {guild.VoiceRegionId}\n";
            stats += $"**Created at:** {guild.CreatedAt:R}\n";
            stats += $"**Verification level:** {guild.VerificationLevel}\n";
            stats += $"**AFK Timeout:** {guild.AFKTimeout} minute(s)\n";
            stats += $"**Icon:** {guild.IconUrl}\n";

            embed.AddField("Generic stats", stats);

            await ReplyAsync(embed: embed.Build());
        }

        ////test code for bot response
        //[Command("reply", RunMode = RunMode.Async)]
        //[Summary("the bot talks back")]
        //public async Task response()
        //{
        //    await ReplyAsync("What is 2+2?");
        //    var response = await NextMessageAsync();
        //    if (response != null)
        //        await ReplyAsync($"You replied: {response.Content}");
        //    else
        //        await ReplyAsync("You did not reply before the timeout");
        //}


        //[Command("alter", RunMode = RunMode.Async)]
        //[Summary("the bot talks back")]
        //public async Task altermessage()
        //{
        //    var Message = await Context.Channel.SendMessageAsync("test message");

        //    await Message.ModifyAsync(msg => msg.Content = "test [edited]");
        //}
    }
}
