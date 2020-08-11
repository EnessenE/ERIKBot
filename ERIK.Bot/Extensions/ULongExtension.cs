using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Extensions
{
    public static class ULongExtension
    {
        public static string ToUserList(this List<ulong> allJoined)
        {
            var allMember = string.Empty;
            foreach (var joinedUser in allJoined)
            {
                allMember += $"\n<@{joinedUser}>";
            }

            return allMember;
        }

    }
}
