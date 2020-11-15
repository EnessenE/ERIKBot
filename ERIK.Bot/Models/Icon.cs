using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Models
{
    public class Icon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Image { get; set; }
        public string Name { get; set; }
        
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        public bool Default { get; set; }
        public bool Active { get; set; }
        public bool Enabled { get; set; }
        public bool Recurring { get; set; }


    }
}
