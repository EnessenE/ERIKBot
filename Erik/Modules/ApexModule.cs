using Discord.Interactions;
using Erik.Configurations;
using Erik.Extensions;
using Microsoft.Extensions.Options;

namespace Erik.Modules
{
    public class ApexModule : InteractionModuleBase
    {
        private readonly ApexConfiguration _apexConfiguration;

        public ApexModule(IOptions<ApexConfiguration> apexOptions)
        {
            _apexConfiguration = apexOptions.Value;
        }

        [SlashCommand("apex", "Choose a random apex character to play")]
        public async Task Apex()
        {
            await RespondAsync(_apexConfiguration.Characters.PickRandom());
        }
    }
}
