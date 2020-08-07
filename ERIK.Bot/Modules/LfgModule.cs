using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Enums;
using ERIK.Bot.Models.Reactions;
using ERIK.Bot.Services;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class LfgModule : ModuleBase<SocketCommandContext>
    {
        private EntityContext _context;

        public LfgModule(EntityContext context)
        {
            _context = context;
        }


        [Command("lfg create")]
        [Summary("Save the last [amount] messages in the selected text channel. Usage: !save [email] [amount (default 100)]")]
        public async Task CreateLfg()
        {
            IUserMessage msg = await ReplyAsync("Created");

            SavedMessage savedMessage = new SavedMessage
            {
                MessageId = msg.Id, 
                IsFinished = false, 
                Type = ReactionMessageType.LFG
            };

            _context.CreateMessage(savedMessage);
            var checkMark = new Emoji("✔️");
            var cross = new Emoji("❌");
            var question = new Emoji("❓");
            var reactions = new IEmote[] { checkMark, cross, question };
            await msg.AddReactionsAsync(reactions); //One call saves data and doesnt hit the API rate limiting

            
        }
    }
}
