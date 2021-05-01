using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERIK.Bot.Models
{
    public class Icon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        ///     The url of the image
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        ///     The name of the icon.
        ///     Used for dumb lookup
        /// </summary>
        public string Name { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool Default { get; set; }

        /// <summary>
        ///     Icon is active and in use
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        ///     Icon is enabled for usage or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Recurring icon, dates have to be set
        /// </summary>
        public bool Recurring { get; set; }
    }
}