using Discord.Addons.Interactive;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Extensions
{
    public class LfgCreateResponceExtention: InteractiveBase
    {
        public async Task Response()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }
    }
}
