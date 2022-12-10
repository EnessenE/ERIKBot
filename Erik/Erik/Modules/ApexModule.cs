using Discord.Interactions;

namespace Erik.Modules
{
    public class ApexModule : InteractionModuleBase
    {
        [SlashCommand("apex", "Choose a random apex character to play")]
        public async Task ThingsAsync()
        {
            await RespondAsync("hai!");
        }
    }
}
