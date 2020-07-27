using System;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using MailSettings = ERIK.Bot.Configurations.MailSettings;

namespace ERIK.Bot.Services
{
    public class MailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly MailSettings _options;

        public MailService(ILogger<MailService> logger, IOptions<MailSettings> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async void SendEmail(string subject, string target, string content, string htmlContent, string fromEmail = "donotreply@reasulus.nl", string fromName = "DO NOT REPLY")
        {
            var client = new SendGridClient(_options.Token);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(target);
            var plainTextContent = content;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation("Success! Result is: {status} - {msg}", response.StatusCode, response.ToString());
        }


        public async void SendTemplateEmail(string templateId, string subject, string target, object data, string fromEmail = "donotreply@reasulus.nl", string fromName = "DO NOT REPLY")
        {
            var client = new SendGridClient(_options.Token);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(target);
            var jsonData = JsonConvert.SerializeObject(data);
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, jsonData);
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation("Success! Result is: {status} - {msg}", response.StatusCode, response.ToString());
        }
    }
}
