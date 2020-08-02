using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ERIK.Bot.Configurations;
using ERIK.Bot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERIK.Bot.Context
{
    public class EntityContext : DbContext
    {
        private readonly IOptions<SQLSettings> _sqlSettings;
        private readonly ILogger<EntityContext> _logger;

        public DbSet<Guild> Guilds { get; set; }

        public EntityContext(IOptions<SQLSettings> sqlSettings, ILogger<EntityContext> logger)
        {
            _sqlSettings = sqlSettings;
            _logger = logger;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_sqlSettings.Value.ConnectionString);
        }

        public Guild GetGuild(string id)
        {
            var result = Guilds.Find(id);
            return result;

        }

        public List<Guild> GetAllGuilds()
        {
            return Guilds.ToList();
        }

    }
}
