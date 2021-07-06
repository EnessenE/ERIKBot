using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ERIK.Bot.Services;
using Victoria;
using Victoria.Enums;

namespace ERIK.Bot.Modules
{
    public class AudioModule : ModuleBase<ICommandContext>
    {
        private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        // Scroll down further for the AudioService.
        // Like, way down
        private readonly AudioService _audioService;
        private readonly LavaNode _lavaNode;


        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public AudioModule(AudioService audioService, LavaNode lavaNode)
        {
            _audioService = audioService;
            _lavaNode = lavaNode;
        }


        [Command("genius", RunMode = RunMode.Async)]
        [Description("Lookup lyrics of a specific track")]
        public async Task ShowGeniusLyrics()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Woaaah there, I'm not playing any tracks.");
                return;
            }

            var lyrics = await player.Track.FetchLyricsFromGeniusAsync();
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await ReplyAsync($"No lyrics found for {player.Track.Title}");
                return;
            }

            var splitLyrics = lyrics.Split('\n');
            var stringBuilder = new StringBuilder();
            foreach (var line in splitLyrics)
                if (Range.Contains(stringBuilder.Length))
                {
                    await ReplyAsync($"```{stringBuilder}```");
                    stringBuilder.Clear();
                }
                else
                {
                    stringBuilder.AppendLine(line);
                }

            await ReplyAsync($"```{stringBuilder}```");
        }


        [Command("soldierboy", RunMode = RunMode.Async)]
        [Description("Plays Soulja Boy Tell'em - Crank That")]
        public async Task SoldierBoy()
        {
            Play("https://www.youtube.com/watch?v=8UFIYGkROII");
        }

        [Command("join")]
        public async Task JoinAndPlay()
        {
            await ReplyAsync(embed: await _audioService.JoinAsync(Context.Guild, Context.User as IVoiceState,
                Context.Channel as ITextChannel));
        }

        [Command("leave")]
        public async Task Leave()
        {
            await ReplyAsync(embed: await _audioService.LeaveAsync(Context.Guild));
        }

        [Command("play")]
        public async Task Play([Remainder] string search)
        {
            await ReplyAsync(
                embed: await _audioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search));
        }

        [Command("stop")]
        public async Task Stop()
        {
            await ReplyAsync(embed: await _audioService.StopAsync(Context.Guild));
        }

        [Command("list")]
        public async Task List()
        {
            await ReplyAsync(embed: await _audioService.ListAsync(Context.Guild));
        }

        [Command("skip")]
        public async Task Skip()
        {
            await ReplyAsync(embed: await _audioService.SkipTrackAsync(Context.Guild));
        }

        [Command("volume")]
        public async Task Volume(int volume)
        {
            await ReplyAsync(await _audioService.SetVolumeAsync(Context.Guild, volume));
        }

        [Command("pause")]
        public async Task Pause()
        {
            await ReplyAsync(await _audioService.PauseAsync(Context.Guild));
        }

        [Command("resume")]
        public async Task Resume()
        {
            await ReplyAsync(await _audioService.ResumeAsync(Context.Guild));
        }
    }
}