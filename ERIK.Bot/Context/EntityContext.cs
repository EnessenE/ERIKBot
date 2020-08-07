using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using ERIK.Bot.Configurations;
using ERIK.Bot.Enums;
using ERIK.Bot.Extensions;
using ERIK.Bot.Models;
using ERIK.Bot.Models.Reactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ERIK.Bot.Context
{
    public class EntityContext : DbContext
    {
        private readonly IOptions<SQLSettings> _sqlSettings;
        private readonly ILogger<EntityContext> _logger;

        public DbSet<SavedMessage> SavedMessages { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<DiscordUser> DiscordUsers { get; set; }

        public EntityContext(IOptions<SQLSettings> sqlSettings, ILogger<EntityContext> logger)
        {
            _sqlSettings = sqlSettings;
            _logger = logger;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_sqlSettings.Value.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SavedMessage>()
                .Property(e => e.TrackedIds)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<ulong>>(v));
        }

        public SavedMessage GetMessage(ulong id)
        {
            var result = SavedMessages.Find(id);
            return result;
        }
        
        public async Task<SavedMessage> GetMessageOnTrackIdAsync(ulong id)
        {
            var data = SavedMessages.AsAsyncEnumerable().Where(a => a.TrackedIds.ContainsItem(id));
            if (await data.AnyAsync())
            {
                return await data.FirstAsync();
            }

            return null;
        }

        public async Task<List<SavedMessage>> GetAllNonPublished(ulong guildId)
        {
            var result = SavedMessages.AsAsyncEnumerable().Where(a => a.Published == false && a.GuildId == guildId);
            return await result.ToListAsync();
        }

        public Guild GetGuild(ulong id)
        {
            var result = Guilds.Find(id);
            if (result != null && result.Id > 0)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public Guild GetOrCreateGuild(ulong id)
        {
            Guild guild = GetGuild(id);
            if (guild != null)
            {
                return guild;
            }
            else
            {
                guild = new Guild()
                {
                    Id = id,
                    Prefix = "!" //retrieve from appsettings
                };
                Add(guild);
                SaveChanges();
            }

            return guild;
        }

        /// <summary>
        /// Automatically tracks it
        /// </summary>
        public SavedMessage CreateMessage(SavedMessage message)
        {
            Add(message);
            SaveChanges();
            return message;
        }

        public void AddReaction(SavedMessage message, IUser user, ReactionState state)
        {
            var retrievedMessage = GetMessage(message.MessageId);
            var foundUser = DiscordUsers.Find(user.Id);
            if (foundUser == null)
            {
                foundUser = new DiscordUser()
                {
                    Id = user.Id
                };
                Add(foundUser);
            }
            MessageReaction item = new MessageReaction
            {
                State = state,
                User = foundUser
            };
            if (retrievedMessage.Reactions == null)
            {
                retrievedMessage.Reactions = new List<MessageReaction>();
            }
            retrievedMessage.Reactions.Add(item);
            Update(retrievedMessage);
            SaveChanges();
        }

        /// <summary>
        /// Clear all reactions of a specific user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user"></param>
        public void RemoveReaction(SavedMessage message, ulong userId)
        {
            var retrievedMessage = GetMessage(message.MessageId);
            if (retrievedMessage.Reactions != null)
            {
                var removalQueue = new List<MessageReaction>();
                foreach (var reaction in retrievedMessage.Reactions)
                {
                    if (reaction.User.Id == userId)
                    {
                        removalQueue.Add(reaction);
                    }
                }

                foreach (var reaction in removalQueue)
                {
                    retrievedMessage.Reactions.Remove(reaction);
                }
            }
        }

        public bool IsTracked(ulong id)
        {
            bool result = false;
            var message = GetMessage(id);
            if (message != null && !message.IsFinished)
            {
                result = true;
            }

            return result;
        }

        public List<SavedMessage> GetAllGuilds()
        {
            return SavedMessages.ToList();
        }

    }
}
