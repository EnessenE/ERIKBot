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
        public async Task GetAllRoles()
        {
            var author = this.Context.Message.Author;

            string message = $"All roles in {this.Context.Guild.Name}: \n";
            foreach (var role in this.Context.Guild.Roles)
            {
                message += role.Name + " - " + role.Members.Count() + " members with this role.\n";
            }

            var splitMessage = message.SplitMessage();

            foreach (var mes in splitMessage)
            {
                await author.SendMessageAsync(mes);
            }
        }
        
    }
}
