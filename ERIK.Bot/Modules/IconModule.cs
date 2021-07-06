using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Handlers;
using ERIK.Bot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class IconModule : InteractiveBase
    {
        private readonly DiscordSocketClient _client;
        private readonly EntityContext _context;
        private readonly Responses _responses;
        private readonly DiscordBotSettings _botSettings;
        private readonly ILogger<IconModule> _logger;


        public IconModule(IOptions<Responses> responses, EntityContext context, ILogger<IconModule> logger,
            IOptions<DiscordBotSettings> botSettings, DiscordSocketClient client)
        {
            _context = context;
            _logger = logger;
            _client = client;
            _botSettings = botSettings.Value;
            _responses = responses.Value;
        }

        [Command("icon list", RunMode = RunMode.Async)]
        [Summary("Lists all current icons.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task List()
        {
            var guild = _context.GetOrCreateGuild(Context.Guild.Id);

            if (guild.IconSupport)
            {
                var fieldsList = new List<EmbedFieldBuilder>();
                var title = "All icons set for this guild";
                var desc = "All times in UTC";
                foreach (var icon in guild.Icons)
                {
                    var date = string.Empty;
                    if (icon.Recurring)
                        date = $"{icon.StartDate.ToLongDateString()} - {icon.EndDate.ToLongDateString()}";
                    else
                        date = "Not recurring.";

                    var setting = $"EN: {icon.Enabled} AC: {icon.Active}";

                    var text = $"{date}\n{setting}\n{icon.Image}";

                    var embedField = new EmbedFieldBuilder
                    {
                        Name = icon.Name,
                        Value = text
                    };
                    fieldsList.Add(embedField);
                }

                var embed = await EmbedHandler.CreateBasicEmbed(title, desc, Color.Green, fieldsList);

                ReplyAsync(null, false, embed);
            }
            else
            {
                //Not enabled.
                ReplyAsync(_responses.NotEnabled.PickRandom() + " - Icon Support");
            }
        }
    }
}