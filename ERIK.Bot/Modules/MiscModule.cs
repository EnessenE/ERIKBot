using System;
using System.Collections;
using System.Collections.Generic;
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
using ERIK.Bot.Handlers;
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
        private readonly EntityContext _context;


        public MiscModule(CommandService commandService, IOptions<Responses> responses, EntityContext context,
            ILogger<MiscModule> logger, CatContext catContext)
        {
            _commandService = commandService;
            _context = context;
            _responses = responses.Value;
            _logger = logger;
            _catContext = catContext;
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
            var guild = Context.Guild;

            var fields = new List<EmbedFieldBuilder>();

            foreach (var p in guild.GetType().GetProperties().Where(p => !p.GetGetMethod().GetParameters().Any()))
            {
                var item = new EmbedFieldBuilder();
                var value = p.GetValue(guild, null);
                item.Name = p.Name;
                item.IsInline = true;
                if (value != null)
                {
                    item.Value = "???";
                    if (typeof(bool).IsAssignableFrom(p.PropertyType) ||
                        typeof(Int32).IsAssignableFrom(p.PropertyType) ||
                        typeof(int).IsAssignableFrom(p.PropertyType) ||
                        typeof(Int16).IsAssignableFrom(p.PropertyType) ||
                        typeof(double).IsAssignableFrom(p.PropertyType) ||
                        typeof(string).IsAssignableFrom(p.PropertyType) ||
                        typeof(DateTimeOffset).IsAssignableFrom(p.PropertyType) ||
                        typeof(DateTime).IsAssignableFrom(p.PropertyType))
                    {
                        item.Value = value.ToString();
                        fields.Add(item);
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                    {
                        int count = ((IReadOnlyCollection<object>)value).Count;

                        item.Value = count.ToString();
                        fields.Add(item);
                    }
                }

            }


            var builtEmbed = await EmbedHandler.CreateBasicEmbed($"Server info {guild.Name}", guild.Description, Color.Blue, fields);
            ReplyAsync(embed: builtEmbed);

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