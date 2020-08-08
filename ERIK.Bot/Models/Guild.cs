using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERIK.Bot.Models
{
    public class Guild
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

        public string Prefix { get; set; }
        public ulong LfgPrepublishChannelId { get; set; }
        public ulong LfgPublishChannelId { get; set; }

    }
}
