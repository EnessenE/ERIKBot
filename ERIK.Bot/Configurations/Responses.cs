using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Configurations
{
    public class Responses
    {
        public List<string> Pong { get; set; }
        public List<string> Martijn { get; set; }

        /// <summary>
        /// Default icon saved to db
        /// </summary>
        public List<string> IconDefault { get; set; }

        /// <summary>
        /// Current icon which should be saved to db is not set
        /// </summary>
        public List<string> IconDefaultWrong { get; set; }

        /// <summary>
        /// Icon has been restored to default
        /// </summary>
        public List<string> IconRestoredToDefault { get; set; }


        /// <summary>
        /// When a method isn't enabled, this message is sent back
        /// </summary>
        public List<string> NotEnabled { get; set; }

        /// <summary>
        /// Couldn't download the picture
        /// </summary>
        public List<string> FailedDownload { get; set; }

    }
}
