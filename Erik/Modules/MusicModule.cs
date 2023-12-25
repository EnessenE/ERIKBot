using System.Collections.Concurrent;
using CliWrap;
using Discord;
using Discord.Audio;
using Discord.Interactions;
using Erik.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;


namespace Erik.Modules
{
    public class MusicModule : InteractionModuleBase
    {
        private readonly MusicConfiguration _musicConfiguration;

        private readonly ILogger<MusicModule> _logger;

        //runtime
        private static readonly IDictionary<ulong, IVoiceChannel> CurrentVoiceChannels =
            new ConcurrentDictionary<ulong, IVoiceChannel>();

        private static readonly IDictionary<ulong, IAudioClient> CurrentAudioClients =
            new ConcurrentDictionary<ulong, IAudioClient>();

        private static readonly IDictionary<ulong, List<QueueData>> Queues =
            new ConcurrentDictionary<ulong, List<QueueData>>();

        private static readonly IDictionary<ulong, bool> GuildData = new ConcurrentDictionary<ulong, bool>();

        private static readonly IDictionary<ulong, CancellationTokenSource> CancellationTokens =
            new ConcurrentDictionary<ulong, CancellationTokenSource>();

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



        [SlashCommand("plays", "Search on youtube for a thing and play it!")]
        public async Task PlayQuery(string text)
        {
            var youtube = new YoutubeClient();
            _logger.LogInformation("search for {text}", text);
            var data = await youtube.Search.GetResultsAsync(text);
            var item = data.FirstOrDefault();
            if (item != null)
            {
                await Play(item.Url);
            }
            else
            {
                await RespondAsync("Found nothing for this query! Sorry.");
            }
        }

        [SlashCommand("play", "Plays this youtube url")]
        public async Task Play(string url)
        {
            var guildUser = (Context.User as IGuildUser);
            try
            {
                if (CurrentVoiceChannels.TryGetValue(Context.Guild.Id, out var client))
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


        [SlashCommand("stop", "Stop playing and clear the queue")]
        public async Task Stop()
        {
            await Skip(true);
        }


        [SlashCommand("skip", "Skip the current song")]
        public async Task Skip(bool skipAll = false)
        {
            if (CancellationTokens.TryGetValue(Context.Guild.Id, out var task))
            {
                if (skipAll)
                {
                    Queues[Context.Guild.Id]?.Clear();
                }
                task.Cancel();
                if (!skipAll)
                {
                    await RespondAsync("Skipped current song");
                }
                else
                {
                    await RespondAsync("Skipped ALL songs.");
                }
            }
            else
            {
                await RespondAsync("Nothing to skip!");
            }
        }


        [SlashCommand("queue", "Shows the current queue")]
        public async Task ShowQueue()
        {
            if (Queues.TryGetValue(Context.Guild.Id, out var queue) && queue.Count > 0)
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
            Queues[Context.Guild.Id]?.Clear();
            CancellationTokens[Context.Guild.Id]?.Cancel();
            await RespondAsync($"Leaving {CurrentVoiceChannels[Context.Guild.Id].Name}!");
            await CurrentVoiceChannels[Context.Guild.Id].DisconnectAsync();
            CurrentVoiceChannels.Remove(Context.Guild.Id);
            CurrentAudioClients.Remove(Context.Guild.Id);
        }


        private async Task<IAudioClient> ConnectToChannel(IGuild guild, IVoiceChannel channel)
        {
            AddToChannel(guild, channel);
            CurrentVoiceChannels.Add(guild.Id, channel);
            var audioClient = await channel.ConnectAsync(false, false, false);
            _logger.LogInformation("Connected to {channel} with a latency of {ms} ms", channel.Name, audioClient.Latency);

            CurrentAudioClients.Add(guild.Id, audioClient);

            return audioClient;
        }

        private void AddToChannel(IGuild guild, IVoiceChannel channel)
        {
            if (CurrentVoiceChannels.ContainsKey(guild.Id))
            {
                _logger.LogInformation("Bot was already in this channel, maybe it was not properly disconnected/ Forcefully removing an audioclient");

                CurrentAudioClients.Remove(guild.Id);
            }
            else
            {
                CurrentVoiceChannels.Add(guild.Id, channel);
            }
        }


        private void AttemptToPlay(ulong guildId)
        {
            var playing = AreWePlaying(guildId);
            if (!playing)
            {
                GuildData[guildId] = true;
                new Task(async () =>
                {
                    while (Queues[guildId].Count > 0)
                    {
                        var source = new CancellationTokenSource();
                        CancellationTokens[guildId] = source;
                        try
                        {
                            var data = Queues[guildId].First();
                            _logger.LogInformation($"Playing {data.GuildUser.Mention} - {data.Title} queue");
                            await Context.Channel.SendMessageAsync($"Playing **{data.Title}** from {data.GuildUser.Mention}");
                            await PlayYoutubeVideo(data, source.Token);
                        }
                        catch (OperationCanceledException error)
                        {
                            _logger.LogError("Something got skipped!");
                        }
                        finally
                        {
                            if (Queues[guildId]?.Count > 0)
                            {
                                Queues[guildId].RemoveAt(0);
                            }
                            await Task.Delay(500);
                        }
                    }
                    GuildData[guildId] = false;
                    CancellationTokens[guildId] = null;
                    _logger.LogInformation("Finished queue");
                }).Start();
            }
        }

        private bool AreWePlaying(ulong guildId)
        {
            if (GuildData.TryGetValue(guildId, out var playing))
            {
                return playing;
            }
            return false;
        }

        private int AddToQueue(ulong guildId, QueueData queueData)
        {
            if (Queues.TryGetValue(guildId, out var data))
            {
                data.Add(queueData);
            }
            else
            {
                Queues.Add(guildId, new List<QueueData>());
                return AddToQueue(guildId, queueData);
            }

            return data.Count;
        }

        private async Task PlayYoutubeVideo(QueueData queueData, CancellationToken token)
        {
            CurrentAudioClients.TryGetValue(Context.Guild.Id, out var client);
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(
                    queueData.Url
                );
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            using var stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            _logger.LogInformation("Starting ffmpeg hax");
            using var memoryStream = new MemoryStream();
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
                    await memoryStream.FlushAsync();
                    await memoryStream.DisposeAsync();
                    await stream.DisposeAsync();
                }
            }
        }
    }
}
