using Discord.Interactions;

namespace Erik.Modules
{
    public class ApexModule : InteractionModuleBase
    {
        [SlashCommand("things", "Shows things")]
        public async Task ThingsAsync()
        {
            await RespondAsync("hai!");
        }
    }
}
