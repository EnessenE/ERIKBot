using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
            try
            {
                var client = new SendGridClient(_options.Token);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(target);
                var plainTextContent = content;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await SendEmail(client, msg);
                _logger.LogInformation("Success! Result is: {status} - {msg}", response.StatusCode, response.ToString());
            }
            catch (Exception error)
            {
                throw error;
            }
        }


        public async void SendTemplateEmail(string templateId, string target, object data, string fromEmail = "donotreply@reasulus.nl", string fromName = "Erik")
        {
            try
            {
                var client = new SendGridClient(_options.Token);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(target);
                var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, data);
                var response = await SendEmail(client, msg);
                _logger.LogInformation("Success! Result is: {status} - {msg}", response.StatusCode,
                    response.ToString());
            }
            catch (Exception error)
            {
                throw;
            }
        }

        private async Task<Response> SendEmail(SendGridClient client, SendGridMessage msg)
        {
            Response response = await client.SendEmailAsync(msg);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                var x = response.DeserializeResponseBodyAsync(response.Body);
                _logger.LogError("Failed to send a email. Error: {code} - {error}", response.StatusCode, JsonConvert.SerializeObject(x));
                throw new Exception("Failed to send email.");
            }

            return response;
        }
    }
}
