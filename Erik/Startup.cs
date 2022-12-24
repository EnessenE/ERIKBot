using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Erik.Managers;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text.Json.Serialization;
using Erik.Configurations;

namespace Erik
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AddConfigurations(services);
            AddDiscordServices(services);
            services.AddHostedService<StatusManager>();

            // ********************
            // Setup CORS
            // ********************
            var corsBuilder = new CorsPolicyBuilder();
            corsBuilder.AllowAnyHeader();
            corsBuilder.AllowAnyMethod();

#if DEBUG
            corsBuilder.WithOrigins("https://reasulus.nl", "http://localhost:4200");
#else
            corsBuilder.WithOrigins("https://reasulus.nl");
#endif

            services.AddCors(options =>
            {
                options.AddPolicy("SiteCorsPolicy", corsBuilder.Build());
            });

            services.AddControllers().AddJsonOptions(x =>
            {
                // serialize enums as strings in api responses (e.g. Role)
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSwaggerGen(c =>
            {
            });
        }

        private void AddConfigurations(IServiceCollection services)
        {
            services.Configure<ApexConfiguration>(Configuration.GetSection("ApexConfiguration"));
            services.Configure<StatusConfiguration>(Configuration.GetSection("StatusConfiguration"));
            services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));
            services.Configure<MusicConfiguration>(Configuration.GetSection("MusicConfiguration"));
        }

        private void AddDiscordServices(IServiceCollection services)
        {
            var config = new DiscordSocketConfig()
            {
                //...
            };

            // X represents either Interaction or Command, as it functions the exact same for both types.
            var servConfig = new CommandServiceConfig()
            {
                //...
            };
            services.AddSingleton(config);
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddHostedService<DiscordClientManager>();
            services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
            services.AddHostedService<InteractionHandler>();
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "reasulus.api"));

            app.UseCors("SiteCorsPolicy");
            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
