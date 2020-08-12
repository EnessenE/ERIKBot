using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ERIK.Bot.Extensions;

namespace ERIK.Bot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("roles")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task GetAllRoles()
        {
            string message = "All roles: \n";
            foreach (var role in this.Context.Guild.Roles)
            {
                message += role.Name + " - " + role.Members.Count() + " members with this role.\n";
            }

            var splitMessage = message.SplitMessage();

            foreach (var mes in splitMessage)
            {
                await ReplyAsync(mes);
            }
        }
        
    }
}
