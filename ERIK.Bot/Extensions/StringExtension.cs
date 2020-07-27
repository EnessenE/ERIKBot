using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERIK.Bot.Extensions
{
    public static  class StringExtension
    {
        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
