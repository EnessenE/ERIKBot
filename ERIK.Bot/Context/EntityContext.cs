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
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    }
}
