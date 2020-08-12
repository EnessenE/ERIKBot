using System;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using ERIK.Bot.Services;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class HistoryModule : ModuleBase<SocketCommandContext>
    {
        private readonly MailService _mailService;
        private readonly MailSettings _options;

        public HistoryModule(MailService mailService, IOptions<MailSettings> options)
        {
            _mailService = mailService;
            _options = options.Value;
        }


        [Command("save")]
        [Summary("Save the last [amount] messages in the selected text channel. Usage: !save [email] [amount (default 100)]")]
        public async Task SaveHistory(string email, int totalMessagesToCollect = 100)
        {
            if (totalMessagesToCollect > 2500)
            {
                totalMessagesToCollect = 2500;
            }
            var author = this.Context.Message.Author;
            var channel = this.Context.Channel;
            var guild = this.Context.Guild;
            var message = this.Context.Message;

            if (email.IsValidEmail())
            {
                await message.DeleteAsync();
                await ReplyAsync("Check your DM's!");

                await author.SendMessageAsync(
                    $"Collecting last {totalMessagesToCollect} messages in the selected channel");
                try
                {
                    var retrievedMessages = await channel.GetMessagesAsync(totalMessagesToCollect).FlattenAsync();
                    var messages = retrievedMessages.ToList();
                    if (messages.Count >= 1)
                    {

                        await author.SendMessageAsync(
                            $"Successfully retrieved {messages.Count} messages in the selected channel.");
                        try
                        {
                            string subject = $"Last {totalMessagesToCollect} chats in {channel.Name}.";
                            HistoryLogData data = new HistoryLogData
                            {
                                subject = subject,
                                guildId = guild.Id.ToString(),
                                guildName = guild.Name,
                                channelId = channel.Id.ToString(),
                                channelName = channel.Name,
                                time = message.Timestamp.ToString(),
                                totalMessages = totalMessagesToCollect.ToString(),
                                totalMessagesSaved = messages.Count.ToString(),
                                requesterName = author.Username
                            };

                            data.logs = messages.ToLogList();

                            _mailService.SendTemplateEmail(_options.HistoryTemplateId, email, data);
                        }
                        catch (Exception error)
                        {
                            await author.SendMessageAsync("Failed to send the email. Is the email valid? \n " +
                                                          error.Message);
                        }

                    }
                    else
                    {
                        throw new NullReferenceException("No messages found");
                    }
                }
                catch (Exception error)
                {
                    await author.SendMessageAsync("I wasn't able to collect the requested messages. \n" +
                                                  error.Message);
                }
            }
            else
            {
                await author.SendMessageAsync("Invalid email. Try again");
            }

        }
    }
}
