using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Extensions
{
    public static class DateTimeExtension
    {
        public static string ToFieldTime(this DateTime time)
        {
            var newTime = time.ToUniversalTime();
            var result = $"{newTime:G} UTC";
            return result;
        }
    }
}
