using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERIK.Bot.Enums;

namespace ERIK.Bot.Models.Reactions
{
    public class MessageReaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DiscordUser User { get; set; }
        public ReactionState State { get; set; }

        [NotMapped]
        public bool HasJoined {
            get
            {
                if (State == ReactionState.Alternate || State == ReactionState.Joined)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
