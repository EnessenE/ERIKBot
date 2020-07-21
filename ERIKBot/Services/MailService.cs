using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace ERIKBot.Services
{
    public class MailService
    {

        public void Start()
        {
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            SmtpServer.Port = 465;
            SmtpServer.EnableSsl = true;
            SmtpServer.Credentials =
                new System.Net.NetworkCredential("enes@reasulus.nl", "$E#Pli65HO1!90u");


            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("DONOTREPLY@reasulus.nl");
            mail.To.Add("mail.enes.nu@gmail.com");
            mail.Subject = "Test email from reasulus";
            mail.Body = "A fully fledged test email";

            SmtpServer.Send(mail);
            Console.WriteLine("Succesfully sent!");
        }
    }
}
