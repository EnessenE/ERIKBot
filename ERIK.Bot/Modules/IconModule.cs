using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class IconModule : InteractiveBase
    {
        private readonly Responses _responses;
        private readonly EntityContext _context;
        private ILogger<IconModule> _logger;
        private readonly DiscordSocketClient _client;
        private DiscordBotSettings _botSettings;


        public IconModule(IOptions<Responses> responses, EntityContext context, ILogger<IconModule> logger, IOptions<DiscordBotSettings> botSettings, DiscordSocketClient client)
        {
            _context = context;
            _logger = logger;
            _client = client;
            _botSettings = botSettings.Value;
            _responses = responses.Value;
        }

        [Command("icon set default")]
        [Summary("Set the current icon as the default icon")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task IconDefault()
        {
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);

            if (guild.IconSupport)
            {

                if (this.Context.Guild.IconUrl != null)
                {
                    _logger.LogDebug("{guild} Found an icon set onto the guild", guild);

                    Icon defaultIcon = null;
                    if (guild.Icons != null)
                    {
                        if (guild.Icons.Count > 0)
                        {
                            _logger.LogDebug("{guild} Found an icon that has been set as default", guild);
                            var value = guild.Icons.First(item => item.Default == true);
                            defaultIcon = value;
                        }
                        else
                        {
                            _logger.LogDebug("{guild} Icon list exists, but empty.", guild.Id);
                        }
                    }
                    else
                    {
                        guild.Icons = new List<Icon>();
                    }

                    if (defaultIcon == null)
                    {
                        _logger.LogDebug("{guild} No default icon set", guild);
                        Icon icon = new Icon
                        {
                            Name = "Default Icon",
                            Default = true,
                            Enabled = true,
                            Image = this.Context.Guild.IconUrl
                        };
                        guild.Icons.Add(icon);
                        _context.Add(icon);
                        _context.SaveChanges();
                        _logger.LogDebug("{guild} Written new icon to entity.", guild);
                    }
                    else
                    {
                        defaultIcon.Image = this.Context.Guild.IconUrl;
                        _context.Update(defaultIcon);
                    }

                    _context.Update(guild);

                    ReplyAsync(_responses.IconDefault.PickRandom());
                }
                else
                {
                    ReplyAsync(_responses.IconDefaultWrong.PickRandom());
                }
            }
            else
            {
                //Not enabled.
                ReplyAsync(_responses.NotEnabled.PickRandom() + " - Icon Support");
            }
        }

        [Command("icon restore")]
        [Summary("Sets the current icon to the default icon.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RestoreDefault()
        {
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);

            if (guild.IconSupport)
            {
                foreach (var icon in guild.Icons) //short hack, fix later by a proper call
                {
                    _logger.LogDebug("Processing Icon [{icon}]", icon.Name);

                    if (icon.Default)
                    {
                        if (icon.Enabled)
                        {
                            SocketGuild socketGuild = _client.GetGuild(guild.Id);

                            //Icon needs to be actived.
                            var filePath = icon.DownloadAndOrGet(_botSettings, guild);

                            await socketGuild.ModifyAsync(x => { x.Icon = new Image(filePath); });
                            ReplyAsync(_responses.IconRestoredToDefault.PickRandom());
                        }
                    }
                }
            }
            else
            {
                //Not enabled.
                ReplyAsync(_responses.NotEnabled.PickRandom() + " - Icon Support");
            }
        }
    }
}
