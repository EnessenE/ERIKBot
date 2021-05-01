using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
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
    public class MiscModule : InteractiveBase
    {
        private readonly CatContext _catContext;
        private readonly CommandService _commandService;
        private readonly ILogger<MiscModule> _logger;
        private readonly Responses _responses;
        private AudioService _audioService;
        private readonly EntityContext _context;


        public MiscModule(CommandService commandService, IOptions<Responses> responses, EntityContext context,
            ILogger<MiscModule> logger, CatContext catContext, AudioService audioService)
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
            var prefix = _context.GetOrCreateGuild(Context.Guild.Id).Prefix;

            var commands = _commandService.Commands.ToList();
            var message = "";

            foreach (var command in commands)
            {
                // Get the command Summary attribute information
                var descriptionText = string.Empty;

                descriptionText += command.Summary ?? "No description available";

                message += $"{prefix}{command.Name}            {descriptionText}\n";
            }

            //replyandeleteasync is broken atm
            var tempMsg = await ReplyAsync("Sent you a PM with all commands!");
            Context.User.SendMessageAsync("Here's a list of commands and their description: ```" + message + "```");
            Context.Message.DeleteAsync();
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

        [Command("prefix")]
        [Summary("Set the prefix")]
        public async Task Prefix(string newPrefix)
        {
            var response = string.Empty;
            var oldPrefix = "ERR";
            var guild = _context.GetOrCreateGuild(Context.Guild.Id);

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
                foreach (var cat in result)
                {
                    _logger.LogDebug("Url to send: {url}", cat.url);
                    await ReplyAsync("Look how cute! " + cat.url);
                }
            else
                ReplyAsync("Couldn't find any cats :(");
        }

        [Command("serverinfo")]
        [Summary("Shows some guild information that was retreived from discord.")]
        public async Task Serverinfo()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var guild = Context.Guild;
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