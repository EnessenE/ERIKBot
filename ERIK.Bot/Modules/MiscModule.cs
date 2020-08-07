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
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
            response = "Saved the new prefix successfully.";
                
            await ReplyAsync(response);
        }

        [Command("embed")]
        public async Task SendRichEmbedAsync()
        {
            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "Hello world!",
                Description = "I am a description set by initializer."
            };
            // Or with methods
            embed.AddField("Field title",
                "Field value. I also support [hyperlink markdown](https://example.com)!");
            embed.WithAuthor(Context.Client.CurrentUser);
            embed.WithFooter(footer => footer.Text = "I am a footer.");
            embed.WithColor(Color.Blue);
            embed.WithTitle("I overwrote \"Hello world!\"");
            embed.WithDescription("I am a description.");
            embed.WithUrl("https://example.com");
            embed.WithCurrentTimestamp();
            var built = embed.Build();
            
            await ReplyAsync(embed: built);
        }
    }
}
