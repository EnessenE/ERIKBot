using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ERIKBot.Modules
{
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;

        public MiscModule(CommandService commandService)
        {
            _commandService = commandService;
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
    }
}
