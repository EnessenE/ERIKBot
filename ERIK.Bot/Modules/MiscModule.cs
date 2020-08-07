using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Options;

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
            Guild guild = _context.GetGuild(this.Context.Guild.Id);
            if (guild != null)
            {
                oldPrefix = guild.Prefix;
                guild.Prefix = newPrefix;
                response = "Saved the new prefix successfully.";
            }
            else
            {
                guild = new Guild()
                {
                    Id = this.Context.Guild.Id,
                    Prefix = newPrefix
                };
                _context.CreateGuild(guild);
                response = "An error occured, couldn't find settings for your guild.";
            }
            await ReplyAsync(response);
        }
    }
}
