using System.Collections.Generic;

namespace ERIK.Bot.Extensions
{
    public static class ULongExtension
    {
        public static string ToUserList(this List<ulong> allJoined)
        {
            var allMember = string.Empty;
            foreach (var joinedUser in allJoined) allMember += $"\n<@{joinedUser}>";

            return allMember;
        }
    }
}