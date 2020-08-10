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
            time = time.ToUniversalTime();
            var result = string.Empty;
            result = time.ToString("g") + " UTC";
            //if (time.Minute != 0)
            //{
            //    result = $"{time.Hour}/{time.Minute} UTC";
            //}
            //else
            //{
            //    result = $"{time.Hour}/{time.Minute}0 UTC";
            //}

            return result;
        }
    }
}
