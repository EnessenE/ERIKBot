using System.Collections.Concurrent;
using CliWrap;
using Discord;
using Discord.Audio;
using Discord.Interactions;
using Erik.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;


namespace Erik.Modules
{
    public class MusicModule : InteractionModuleBase
    {
        private readonly MusicConfiguration _musicConfiguration;

        private readonly ILogger<MusicModule> _logger;
        //runtime
        private static readonly IDictionary<ulong, IVoiceChannel> _currentVoiceChannels = new ConcurrentDictionary<ulong, IVoiceChannel>();
        private static readonly IDictionary<ulong, IAudioClient> _currentAudioClients = new ConcurrentDictionary<ulong, IAudioClient>();
        private static readonly IDictionary<ulong, List<QueueData>> _queues = new ConcurrentDictionary<ulong, List<QueueData>>();
        private static readonly IDictionary<ulong, bool> _guildData = new ConcurrentDictionary<ulong, bool>();
        private static readonly IDictionary<ulong, CancellationTokenSource> _cancellationTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

        public MusicModule(IOptions<MusicConfiguration> apexOptions, ILogger<MusicModule> logger)
        {
            _logger = logger;
            _musicConfiguration = apexOptions.Value;
        }

        [SlashCommand("join", "Join a channel that you are in")]
        public async Task JoinChannel()
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            _logger.LogInformation("Requested to join channel {channel}", channel);

            if (channel == null)
            {
                await ReplyAsync("You are not in a voice channel so I can not follow you");
            }
            else
            {
                await RespondAsync($"Joining {channel.Name}!");
                await ConnectToChannel(Context.Guild, channel);
            }

        }

        [SlashCommand("play", "Plays this youtube url")]
        public async Task Play(string url)
        {
            var guildUser = (Context.User as IGuildUser);
            try
            {
                if (_currentVoiceChannels.TryGetValue(Context.Guild.Id, out var client))
                {
                    var youtube = new YoutubeClient();

                    var video = await youtube.Videos.GetAsync(url);

                    if (video.Duration <= TimeSpan.FromMinutes(_musicConfiguration.MaxDuration))
                    {
                        var queueData = new QueueData()
                        {
                            GuildUser = guildUser,
                            Url = url,
                            Title = video.Title
                        };
                        var position = AddToQueue(Context.Guild.Id, queueData);
                        await RespondAsync($"Added **{video.Title}** from {guildUser?.Mention} to the queue in position {position}.");

                        AttemptToPlay(Context.Guild.Id);
                        _logger.LogInformation("Finished!");
                    }
                    else
                    {
                        await RespondAsync($"I ***can't*** play video's longer then {_musicConfiguration.MaxDuration} minutes, so I am not playing **{video.Title}**");
                    }
                }
                else
                {
                    await RespondAsync("I first need to join a channel, then I can start playing things");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to play music!");
                await ReplyAsync("SOMETHING REALLY WENT WRONG");
            }
        }

        [SlashCommand("skip", "Skip the current song")]
        public async Task Skip()
        {
            if (_cancellationTokens.TryGetValue(Context.Guild.Id, out var task))
            {
                task.Cancel();
                await RespondAsync("Skipped current song");
            }
            else
            {
                await RespondAsync("Nothing to skip!");
            }
        }


        [SlashCommand("queue", "Shows the current queue")]
        public async Task showQueue()
        {
            if (_queues.TryGetValue(Context.Guild.Id, out var queue) && queue.Count > 0)
            {
                var text = "The queue:\n";
                for (int i = 0; i < queue.Count; i++)
                {
                    var data = queue[i];
                    text += $"[{i}] {data.GuildUser.Mention} - {data.Title}\n";
                }
                await RespondAsync(text);
            }
            else
            {
                await RespondAsync("There is no queue!");
            }

        }


        [SlashCommand("leave", "Leaves a channel")]
        public async Task LeaveChannel()
        {
            await RespondAsync($"Leaving {_currentVoiceChannels[Context.Guild.Id].Name}!");
            await _currentVoiceChannels[Context.Guild.Id].DisconnectAsync();
            _currentVoiceChannels.Remove(Context.Guild.Id);
            _currentAudioClients.Remove(Context.Guild.Id);
        }


        private async Task<IAudioClient> ConnectToChannel(IGuild guild, IVoiceChannel channel)
        {
            _currentVoiceChannels.Add(guild.Id, channel);
            var audioClient = await channel.ConnectAsync(false, false, false);
            _logger.LogInformation("Connected to {channel} with a latency of {ms} ms", channel.Name, audioClient.Latency);

            _currentAudioClients.Add(guild.Id, audioClient);

            return audioClient;
        }


        private void AttemptToPlay(ulong guildId)
        {
            var playing = AreWePlaying(guildId);
            if (!playing)
            {
                _guildData[guildId] = true;
                new Task(async () =>
                {
                    while (_queues[guildId].Count > 0)
                    {
                        var source = new CancellationTokenSource();
                        _cancellationTokens[guildId] = source;
                        try
                        {
                            var data = _queues[guildId].First();
                            _logger.LogInformation($"Playing {data.GuildUser.Mention} - {data.Title} queue");
                            await PlayYoutubeVideo(data, source.Token);
                        }
                        catch (OperationCanceledException error)
                        {
                            _logger.LogError("Something got skipped!");
                        }
                        finally
                        {
                            _queues[guildId].RemoveAt(0);
                            await Task.Delay(500);
                        }
                    }
                    _cancellationTokens[guildId] = null;
                    _logger.LogInformation("Finished queue");
                }).Start();
            }
        }

        private bool AreWePlaying(ulong guildId)
        {
            if (_guildData.TryGetValue(guildId, out var playing))
            {
                return playing;
            }
            return false;
        }

        private int AddToQueue(ulong guildId, QueueData queueData)
        {
            if (_queues.TryGetValue(guildId, out var data))
            {
                data.Add(queueData);
            }
            else
            {
                _queues.Add(guildId, new List<QueueData>());
                return AddToQueue(guildId, queueData);
            }

            return data.Count;
        }

        private async Task PlayYoutubeVideo(QueueData queueData, CancellationToken token)
        {
            _currentAudioClients.TryGetValue(Context.Guild.Id, out var client);
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(
                    queueData.Url
                );
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            _logger.LogInformation("Starting ffmpeg hax");
            var memoryStream = new MemoryStream();
            await Cli.Wrap("ffmpeg")
                    .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                    .WithStandardInputPipe(PipeSource.FromStream(stream))
                    .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                    .ExecuteAsync();

            _logger.LogInformation("Finished ffmpeg hax. Got a memory stream worth {count} bytes",
                    memoryStream.Length);
            using (var audioStream = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try
                {
                    await audioStream.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length, token);
                }
                catch (OperationCanceledException error)
                {
                    throw;
                }
                finally
                {
                    await audioStream.FlushAsync();
                }
            }
        }
    }

    internal class GuildData
    {
        public bool Playing { get; set; }
    }

    internal class QueueData
    {
        public IGuildUser GuildUser { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
    }
}
