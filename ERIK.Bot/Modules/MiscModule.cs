using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERIK.Bot.Configurations;
using ERIK.Bot.Extensions;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly Responses _responses;

        public MiscModule(CommandService commandService, IOptions<Responses> responses)
        {
            _commandService = commandService;
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
    }
}
