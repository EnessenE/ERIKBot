using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Models
{
    public class Guild
    {
        [Key]
        public string Id { get; set; }
        
        public string Prefix { get; set; }
    }
}
