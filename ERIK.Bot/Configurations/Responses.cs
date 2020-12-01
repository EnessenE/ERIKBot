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
        public List<string> IconDefault { get; set; }
        public List<string> IconDefaultWrong { get; set; }
        
        /// <summary>
        /// When a method isn't enabled, this message is sent back
        /// </summary>
        public List<string> NotEnabled { get; set; }

    }
}
