using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ERIKBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ERIKBot
{
    public class Startup
    {
        public Startup(string[] args)
        {
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
            services.AddTransient<BotService>();
            //services.AddTransient<ClientStatusModule>(); //not an actual component of Discord.Net but something added seperatly
        }
    }
}
