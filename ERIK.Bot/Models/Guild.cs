using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERIK.Bot.Models
{
    public class Guild
    {
        [Key]
        public ulong Id { get; set; }

        public string Prefix { get; set; }

        //features
        public bool IconSupport { get; set; }

        public virtual List<Icon> Icons { get; set; }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
