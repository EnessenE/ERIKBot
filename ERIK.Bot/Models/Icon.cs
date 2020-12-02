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
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool Default { get; set; }

        /// <summary>
        /// Icon is active and in use
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Icon is enabled for usage or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Recurring icon, dates have to be set
        /// </summary>
        public bool Recurring { get; set; }


    }
}
