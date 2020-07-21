using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ERIKBot.Configurations;
using ERIKBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERIKBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();             // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();     // Build the service provider

            await provider.GetRequiredService<BotService>().Start();       // Start the startup service
            await Task.Delay(-1);                               // Keep the program alive
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DiscordBotSettings>(Configuration.GetSection("DiscordBot"));

            services.AddTransient<BotService>();
            //services.AddTransient<ClientStatusModule>(); //not an actual component of Discord.Net but something added seperatly
        }
    }
}
