using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERIK.Bot.Enums;

namespace ERIK.Bot.Models.Reactions
{
    public class SavedMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public List<MessageReaction> Reactions { get; set; }
        public List<TrackedMessage> TrackedIds { get; set; }

        public ulong GuildId { get; set; }
        public bool IsFinished { get; set; }
        public bool Published { get; set; }

        //LFG
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
        public ulong AuthorId { get; set; }

        public ReactionMessageType Type { get; set; }

    }
}
