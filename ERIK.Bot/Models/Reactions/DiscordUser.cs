using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace ERIK.Bot.Models.Reactions
{
    public class DiscordUser
    {
        [Key]
        public ulong Id { get; set; }
    }
}
