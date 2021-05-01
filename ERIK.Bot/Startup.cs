using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Handlers;
using ERIK.Bot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.Generation.Processors.Security;
using Serilog;
using Victoria;

namespace ERIK.Bot
{
    public class Startup
    {
        //Great implementation by Victoria
        public static LavaNode LavaNode;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DiscordBotSettings>(Configuration.GetSection("DiscordBot"));
            services.Configure<SQLSettings>(Configuration.GetSection("SQLSettings"));
            services.Configure<Responses>(Configuration.GetSection("Responses"));

            services.AddSingleton<DiscordSocketClient>();


            services.AddSingleton(services);

            //services.AddSingleton<InteractiveService>();
            services.AddTransient<BotService>();
            services.AddTransient<ReactionService>();
            services.AddTransient<SpecialStuffHandler>();
            services.AddTransient<CatContext>();

            services.AddSingleton<AudioService>();

            services.AddSingleton<LavaNode>();
            services.AddSingleton<LavaConfig>();

            services.AddTransient<EntityContext>();

            //Add the DBContext
            services.AddDbContext<EntityContext>();

            //Add logging
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            //Add automated API doc generation
            services.AddSwaggerDocument(document =>
            {
                document.Title = "ERIK API";

                document.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                });

                document.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("bearer"));
            });

            services.AddCors(o => o.AddPolicy("OpenPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
            services.AddControllers();
        }

        public async void RunAsync()
        {
            var services = new ServiceCollection(); // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider(); // Build the service provider

            await provider.GetRequiredService<BotService>().Start(provider); // Start the startup service

            await Task.Delay(-1); // Keep the program alive
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("OpenPolicy");

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            RunAsync();
        }
    }
}