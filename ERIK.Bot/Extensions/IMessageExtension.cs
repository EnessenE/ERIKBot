using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using ERIK.Bot.Models;

namespace ERIK.Bot.Extensions
{
    public static class IMessageExtension
    {
        public static Log ToLog(this IMessage message)
        {
            Log log = new Log
            {
                authorTag = message.Author.Username,
                message = message.Content,
                simplifiedTime = message.Timestamp.ToString("HH:mm:ss"),
                edited = (message.EditedTimestamp != null)
            };
            return log;
        }

        public static List<Log> ToLogList(this List<IMessage> messages)
        {
            List<Log> logs = new List<Log>();
            foreach (var message in messages)
            {
                logs.Add(message.ToLog());
            }

            return logs;
        }

    }
}
