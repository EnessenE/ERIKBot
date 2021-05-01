using System.Collections.Generic;
using System.Linq;
using ERIK.Bot.Configurations;
using ERIK.Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Context
{
    public class EntityContext : DbContext
    {
        private readonly ILogger<EntityContext> _logger;
        private readonly IOptions<SQLSettings> _sqlSettings;

        public EntityContext(IOptions<SQLSettings> sqlSettings, ILogger<EntityContext> logger)
        {
            _sqlSettings = sqlSettings;
            _logger = logger;
        }

        public DbSet<Guild> Guilds { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseSqlServer(_sqlSettings.Value.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public Guild GetGuild(ulong id)
        {
            var result = Guilds.Find(id);
            if (result != null && result.Id > 0)
                return result;
            return null;
        }

        public Guild GetOrCreateGuild(ulong id)
        {
            var guild = GetGuild(id);
            if (guild != null)
            {
                return guild;
            }

            guild = new Guild
            {
                Id = id,
                Prefix = "!" //TODO: retrieve from appsettings
            };
            Add(guild);
            SaveChanges();

            return guild;
        }

        /// <summary>
        ///     Returns all guilds known to the bot
        /// </summary>
        /// <returns></returns>
        public List<Guild> GetGuilds()
        {
            return Guilds.ToList();
        }

        public List<Icon> GetIcons(Guild guild)
        {
            return guild.Icons;
        }
    }
}