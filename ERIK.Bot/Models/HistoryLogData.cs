using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Models
{
    public class HistoryLogData
    {
        public string subject { get; set; }
        public string guildName { get; set; }
        public string guildId { get; set; }
        public string channelName { get; set; }
        public string channelId { get; set; }
        public string time { get; set; }
        public string totalMessages { get; set; }
        public string totalMessagesSaved { get; set; }
        public string requesterName { get; set; }

        public List<Log> logs { get; set; }

    }

    public class Log
    {
        public string simplifiedTime { get; set; }
        public string authorTag { get; set; }
        public string message { get; set; }
        public bool edited { get; set; }
    }
}
