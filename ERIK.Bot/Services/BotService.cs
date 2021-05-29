using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Configurations;
using ERIK.Bot.Context;
using ERIK.Bot.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Victoria;

namespace ERIK.Bot.Services
{
    internal class BotService : IHostedService
    {
        private readonly DiscordBotSettings _botOptions;
        private readonly ILogger<BotService> _logger;
        private readonly ReactionService _reactionService;
        private readonly IServiceCollection _services;
        private readonly SpecialStuffHandler _specialStuffHandler;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private LavaNode _lavaNode;
        private IServiceProvider _serviceProvider;

        public BotService(ILogger<BotService> logger, IOptions<DiscordBotSettings> botOptions,
            ReactionService reactionService, IServiceCollection services, SpecialStuffHandler specialStuffHandler, IServiceProvider serviceProvider, LavaNode lavaNode)
        {
            _logger = logger;
            _botOptions = botOptions.Value;
            _reactionService = reactionService;
            _services = services;
            _specialStuffHandler = specialStuffHandler;
            _serviceProvider = serviceProvider;
            _lavaNode = lavaNode;
        }

        public async Task InstallCommandsAsync()
        {
            _client.Ready += OnReadyAsync;
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += HandleReaction;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task OnReadyAsync()
        {
            if (!_lavaNode.IsConnected) _lavaNode.ConnectAsync();
        }

        private Task HandleReaction(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel,
            SocketReaction socketReaction)
        {
            return _reactionService.HandleReactionAsync(_client, cachedMessage, channel, socketReaction);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            var channel = message.Channel as SocketGuildChannel;
            //check first if author isn't a bot
            if (message == null || message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);

            if (channel == null)
            {
                //ugly
                await context.Channel.SendMessageAsync("Sorry, I don't respond here");
                return;
            }

            var guild = channel.Guild;


            _logger.LogInformation("[{time}]{author}: {content}", message.Timestamp.ToString("HH:mm:ss"),
                message.Author.Username, message.Content);

            try
            {
                _specialStuffHandler.MessageChannelCheck(context, message, guild);
            }
            catch (Exception error)
            {
                _logger.LogError("Failed executing special stuff", error);
            }

            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            var tempContext = _serviceProvider.GetRequiredService<EntityContext>();
            var prefix = tempContext.GetOrCreateGuild(guild.Id).Prefix;

            if (!(message.HasStringPrefix(prefix, ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;

            // Create a WebSocket-based command context based on the message

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            try
            {
                _logger.LogInformation($"[CMD] Accepted command: {message.Content}");
                var result = await _commands.ExecuteAsync(
                    context,
                    argPos,
                    _serviceProvider);
                if (result.IsSuccess) _logger.LogInformation("[CMD] Successfully executed command");
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    _logger.LogError($"[CMD] Failed executing your command.  {result.ErrorReason}");

                    await context.Channel.SendMessageAsync("Failed executing your command. \n" + result.ErrorReason);
                }
            }
            catch (Exception error)
            {
                await context.Channel.SendMessageAsync("Failed executing your command. \n" + error.Message);
            }


            // Optionally, we may inform the user if the command fails
            // to be executed; however, this may not always be desired,
            // as it may clog up the request queue should a user spam a
            // command.
        }

        private Task Log(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(msg.ToString());
                    break;
                case LogSeverity.Error:
                    _logger.LogError(msg.ToString());
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(msg.ToString());
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(msg.ToString());
                    break;
                case LogSeverity.Verbose:
                    _logger.LogInformation(msg.ToString());
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(msg.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new DiscordSocketClient();
            var config = new CommandServiceConfig();

            _commands = new CommandService(config);

            //logger
            _client.Log += Log;

            _services.AddSingleton(_client);

            _logger.LogInformation($"Attempting to connect to lavalink host: {_botOptions.LavaHost}");
            _services.AddLavaNode(x =>
            {
                x.SelfDeaf = false;
                x.Authorization = _botOptions.LavaKey;
                x.Hostname = _botOptions.LavaHost;
            });
            //We need to retrieve this later so we can start the bot

            //connect events
            InstallCommandsAsync();

            //end

            await _client.LoginAsync(TokenType.Bot, _botOptions.Token);
            await _client.StartAsync();

            //Client has started
            //_logger.LogInformation($"I am {_client.CurrentUser.Username}.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.StopAsync();
            return Task.CompletedTask;
        }
    }
}