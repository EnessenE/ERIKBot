using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Services
{
    public class IconService
    {
        private readonly EntityContext _context;
        private readonly ILogger<IconService> _logger;
        private readonly DiscordSocketClient _client;
        private readonly DiscordBotSettings _botSettings;

        public IconService(EntityContext context, ILogger<IconService> logger, DiscordSocketClient client, IOptions<DiscordBotSettings> botSettings)
        {
            _context = context;
            _logger = logger;
            _client = client;
            _botSettings = botSettings.Value;
        }

        public void Start()
        {
            Thread thread = new Thread(Loop);
            thread.Name = "Icon Thread";
            thread.Start();
        }

        private void Loop()
        {
            while (true)
            {
                CheckForIconChanges();
                Thread.Sleep(60 * 1000);
            }
        }

        public void CheckForIconChanges()
        {
            _logger.LogInformation("Starting to check icon changes");
            var guilds = _context.GetGuilds();

            _logger.LogInformation("Checking for icon changes in {number} guilds", guilds.Count);

            foreach (var guild in guilds)
            {
                if (guild.IconSupport)
                {
                    _logger.LogDebug("{id} has icon support enabled", guild.Id);
                    Task.Run(() => { CheckIcons(guild).ConfigureAwait(false); });
                }
            }
        }

        public async Task CheckIcons(Guild guild)
        {
            var goToDefault = false;
            Icon defaultIcon = null;
            var icons = _context.GetIcons(guild);

            if (icons != null)
            {
                _logger.LogDebug("{guild} has {icons} icons", guild.Id, icons.Count);

                foreach (var icon in icons)
                {
                    _logger.LogDebug("{guild} Processing Icon [{icon}]", guild, icon.Name);

                    if (!icon.Default)
                    {
                        if (icon.Enabled)
                        {
                            if (icon.StartDate >= DateTime.Now && icon.EndDate <= DateTime.Now)
                            {
                                SocketGuild socketGuild = _client.GetGuild(guild.Id);

                                //Icon needs to be actived.
                                var filePath = icon.DownloadAndOrGet(_botSettings, guild);

                                await socketGuild.ModifyAsync(x => { x.Icon = new Image(filePath); });
                                _logger.LogDebug("{guild} Activating icon [{icon}]", guild, icon.Name);

                            }
                        }
                        else if (icon.Active)
                        {
                            //Icon is currently active, and needs to be disabled
                            if (icon.EndDate >= DateTime.Now)
                            {
                                goToDefault = true;
                                if (!icon.Recurring)
                                {
                                    icon.Active = false;
                                    icon.Enabled = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        defaultIcon = icon;
                        _logger.LogDebug("{guild} Is default [{icon}]", guild, icon.Name);

                    }

                    _logger.LogDebug("{guild} Completed Icon [{icon}]", guild, icon.Name);
                }

                if (goToDefault)
                {
                    SocketGuild socketGuild = _client.GetGuild(guild.Id);
                    if (socketGuild == null)
                    {
                        _logger.LogError("Failed retrieving guild {guild}", guild.Id);
                    }
                    else
                    {
                        if (defaultIcon != null)
                        {
                            var filePath = defaultIcon.DownloadAndOrGet(_botSettings, guild);

                            await socketGuild.ModifyAsync(x => { x.Icon = new Image(filePath); });
                        }
                        else
                        {
                            _logger.LogError("Guild {id} hasn't set an default icon", guild.Id);
                        }
                    }
                }
            }
            else
            {
                _logger.LogDebug("No icons found for guild {guild}", guild.Id);
            }
        }
    }
}
