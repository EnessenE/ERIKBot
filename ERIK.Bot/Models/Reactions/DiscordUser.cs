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
        public long __Id { get; set; }


        // Access/modify this variable instead.
        // Tell EF not to map this field to a Db table
        [NotMapped]
        public ulong Id
        {
            get
            {
                unchecked
                {
                    return (ulong)__Id;
                }
            }

            set
            {
                unchecked
                {
                    __Id = (long)value;
                }
            }
        }
    }
}
