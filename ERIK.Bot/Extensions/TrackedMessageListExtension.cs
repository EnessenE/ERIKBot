using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERIK.Bot.Models.Reactions;

namespace ERIK.Bot.Extensions
{
    public static class TrackedMessageListExtension
    {
        public static bool ContainsItem(this List<TrackedMessage> list, ulong target)
        {
            foreach (var item in list)
            {
                if (item.MessageId == target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
