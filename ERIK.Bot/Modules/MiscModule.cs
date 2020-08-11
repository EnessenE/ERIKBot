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
    public class MiscModule : InteractiveBase
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
            string response = string.Empty;
            string oldPrefix = "ERR";
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
            response = "Saved the new prefix successfully.";

            await ReplyAsync(response);
        }

        [Command("serverinfo")]
        [Summary("Set the prefix")]
        public async Task Serverinfo()
        {
            var guild = this.Context.Guild;
            var embed = new EmbedBuilder();
            embed.WithThumbnailUrl(guild.IconUrl);
            embed.WithTitle($"Server info {guild.Name}");
            embed.WithDescription(guild.Description);
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

        //test code for bot response
        [Command("reply", RunMode = RunMode.Async)]
        [Summary("the bot talks back")]
        public async Task response()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }


        [Command("alter", RunMode = RunMode.Async)]
        [Summary("the bot talks back")]
        public async Task altermessage()
        {
            var Message = await Context.Channel.SendMessageAsync("test message");
            
            await Message.ModifyAsync(msg => msg.Content = "test [edited]");
        }
    }
}
