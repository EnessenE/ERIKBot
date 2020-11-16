using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class IconModule : InteractiveBase
    {
        private readonly CommandService _commandService;
        private readonly Responses _responses;
        private readonly EntityContext _context;


        public IconModule(IOptions<Responses> responses, EntityContext context, CommandService commandService)
        {
            _context = context;
            _commandService = commandService;
            _responses = responses.Value;
        }

        [Command("icon default")]
        [Summary("Sets the current icon to default.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task IconDefault()
        {
            if (this.Context.Guild.IconUrl != null)
            {
                Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
                Icon defaultIcon = null;
                if (guild.Icons != null)
                {
                    if (guild.Icons.Count > 0)
                    {
                        var value = guild.Icons.First(item => item.Default == true);
                        defaultIcon = value;

                    }
                }
                else
                {
                    guild.Icons = new List<Icon>();
                }

                if (defaultIcon == null)
                {
                    Icon icon = new Icon
                    {
                        Name = "Default Icon",
                        Default = true,
                        Enabled = true
                    };
                    icon.Image = this.Context.Guild.IconUrl;
                    guild.Icons.Add(icon);
                    _context.Add(icon);
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

    }
}
