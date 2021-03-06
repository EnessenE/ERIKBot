using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ERIK.Bot.Services;

namespace ERIK.Bot.Modules
{
    public class AudioModule : ModuleBase<ICommandContext>
    {
        // Scroll down further for the AudioService.
        // Like, way down
        private readonly AudioService _service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot
        public AudioModule(AudioService service)
        {
            _service = service;
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCmd([Remainder] string song)
        {
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
        }

        //[Command("soldierboy", RunMode = RunMode.Async)]
        //[Summary("Plays Soulja Boy Tell'em - Crank That (Soulja Boy)")]
        //public async Task SoldierBoy()
        //{
        //    await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        //    await _service.SendAudioAsync(Context.Guild, Context.Channel, "https://www.youtube.com/watch?v=8UFIYGkROII");
        //}
    }
}
