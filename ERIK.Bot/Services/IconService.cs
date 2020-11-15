using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIK.Bot.Context;
using ERIK.Bot.Models;
using Microsoft.Extensions.Logging;

namespace ERIK.Bot.Services
{
    public class IconService
    {
        private readonly EntityContext _context;
        private readonly ILogger<IconService> _logger;
        private readonly DiscordSocketClient _client;

        public IconService(EntityContext context, ILogger<IconService> logger, DiscordSocketClient client)
        {
            _context = context;
            _logger = logger;
            _client = client;
        }

        public void CheckForIconChanges()
        {
            foreach (var guild in _context.GetGuilds())
            {
                if (guild.IconSupport)
                {
                    CheckIcons(guild);
                }
            }
        }

        public void CheckIcons(Guild guild)
        {
            foreach (var icon in _context.GetIcons(guild))
            {
                _logger.LogDebug("Processing Icon [{icon}]", icon.Name);

                if (!icon.Default)
                {
                    if (icon.Enabled)
                    {
                        if (icon.startDate >= DateTime.Now && icon.endDate <= DateTime.Now)
                        {

                        }
                    }
                    else if (icon.Active)
                    {
                        //Icon is currently active, and needs to be disabled
                        if (icon.endDate >= DateTime.Now)
                        {

                        }
                    }
                }

                _logger.LogDebug("Completed Icon [{icon}]", icon.Name);
            }
        }
    }
}
