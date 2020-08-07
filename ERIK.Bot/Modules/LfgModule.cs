using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Enums;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using ERIK.Bot.Models.Reactions;
using ERIK.Bot.Services;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Modules
{
    public class LfgModule : ModuleBase<SocketCommandContext>
    {
        private readonly EntityContext _context;

        public LfgModule(EntityContext context)
        {
            _context = context;
        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg create")]
        [Summary("Save the last [amount] messages in the selected text channel. Usage: !save [email] [amount (default 100)]")]
        public async Task CreateLfg(string activity, string desc, DateTime time)
        {
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);

            if (this.Context.Channel.Id != guild.LfgPrepublishChannelId)
            {
                await ReplyAsync("This command can only be used in the pre-publish channel");
                return;
            }
            SavedMessage savedMessage = new SavedMessage
            {
                IsFinished = false,
                Type = ReactionMessageType.LFG,
                Time = time,
                Title = activity,
                Description = desc,
                GuildId = this.Context.Guild.Id
            };

            IUserMessage msg = await ReplyAsync(embed: savedMessage.ToEmbed(this.Context.Client));

            savedMessage.TrackedIds = new List<TrackedMessage>();
            savedMessage.TrackedIds.Add(new TrackedMessage() { ChannelId = msg.Channel.Id, MessageId = msg.Id });

            _context.CreateMessage(savedMessage);
            await ConnectMessage(savedMessage, msg);

        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg prepublish")]
        [Summary("Before publishing an LFG you can pre-create it in a channel")]
        public async Task SetChannelPrePublish(IChannel channel)
        {
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
            guild.LfgPrepublishChannelId = channel.Id;
            _context.SaveChanges();
            await ReplyAsync($"Set the new pre-publish channel to <@{channel.Id}>");
        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg normalchannel")]
        [Summary("Before publishing an LFG you can pre-create it in a channel")]
        public async Task SetChannelPublish(IChannel channel)
        {
            Guild guild = _context.GetOrCreateGuild(this.Context.Guild.Id);
            guild.LfgPublishChannelId = channel.Id;
            _context.SaveChanges();
            await ReplyAsync($"Set the new publish channel to <@{channel.Id}>");
        }

        [RequireUserPermission(GuildPermission.MentionEveryone)]
        [Command("lfg publish")]
        [Summary("Publish all current pre-published LFGs to the pre-selected publish channel")]
        public async Task Publish()
        {
            List<SavedMessage> messages = await _context.GetAllNonPublished(this.Context.Guild.Id);
            var targetChannel = _context.GetOrCreateGuild(this.Context.Guild.Id).LfgPublishChannelId;
            if (messages != null && messages.Count > 0)
            {
                if (targetChannel > 0)
                {
                    var channel = this.Context.Client.GetChannel(targetChannel) as IMessageChannel;
                    foreach (var message in messages)
                    {
                        message.Published = true;
                        IUserMessage sentMessage = await channel.SendMessageAsync(embed: message.ToEmbed(this.Context.Client));
                        message.TrackedIds.Add(new TrackedMessage()
                        {
                            ChannelId = sentMessage.Channel.Id,
                            MessageId = sentMessage.Id
                        });

                        await ConnectMessage(message, sentMessage);

                        _context.UpdateRange(messages);
                        _context.SaveChanges();
                        await ReplyAsync("I successfully published all non published LFG posts for this guild.");
                    }
                }
                else
                {
                    await ReplyAsync("No publish channel set.");
                }
            }
            else
            {
                await ReplyAsync("No posts to publish.");
            }

        }

        private async Task ConnectMessage(SavedMessage message, IUserMessage sentMessage)
        {
            var checkMark = new Emoji("✔️");
            var cross = new Emoji("❌");
            var question = new Emoji("❓");
            var reactions = new IEmote[] { checkMark, cross, question };
            await sentMessage.AddReactionsAsync(reactions); //One call saves data and doesn't hit the API rate limiting
        }

    }
}
