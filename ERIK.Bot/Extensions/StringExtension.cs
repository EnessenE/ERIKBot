using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace ERIK.Bot.Extensions
{
    public static class StringExtension
    {
        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string FilterMessage(this string message)
        {
            message = message.Replace("@everyone", "@-everyone");
            return message;
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filterMessage">Should the message automaticly be filtered</param>
        /// <returns></returns>
        public static List<string> SplitMessage(this string message, bool filterMessage = true)
        {
            if (filterMessage) message = message.FilterMessage();

            return message.Split(1999).ToList();
        }

        public static IEnumerable<string> Split(this string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }
    }
}