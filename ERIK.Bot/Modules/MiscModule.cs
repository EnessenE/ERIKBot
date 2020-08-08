using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using ERIK.Bot.Models.Reactions;

namespace ERIK.Bot.Modules
{
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly Responses _responses;
        private EntityContext _context;
        

        public MiscModule(CommandService commandService, IOptions<Responses> responses, EntityContext context)
        {
            _commandService = commandService;
            _context = context;
            _responses = responses.Value;
        }

        [Command("help")]
        [Summary("A summary of all available commands")]
        public async Task Help()
        {
            List<CommandInfo> commands = _commandService.Commands.ToList();
            EmbedBuilder embedBuilder = new EmbedBuilder();

            foreach (CommandInfo command in commands)
            {
                // Get the command Summary attribute information
                string embedFieldText = command.Summary ?? "No description available\n";

                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }

        [Command("ping")]
        [Summary("Send a response")]
        public async Task Pong()
        {
            await ReplyAsync(_responses.Pong.PickRandom());
        }

        [Command("prefix")]
        [Summary("Set the prefix")]
        public async Task Prefix(string newPrefix)
        {
            string response = string.Empty;
            string oldPrefix = "ERR";
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
            response = "Saved the new prefix successfully.";
                
            await ReplyAsync(response);
        }

        [Command("response")]
        [Summary("back and fort")]
        public async Task Respond()
        {
            await ReplyAsync("What is 2+2?");
            //LfgCreateResponceExtention lfgCRE = new LfgCreateResponceExtention();

            //await lfgCRE.Response();
        }

    }
}
