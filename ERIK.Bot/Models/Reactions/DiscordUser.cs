using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace ERIK.Bot.Models.Reactions
{
    public class DiscordUser : IUser
    {
        [Key]
        public ulong Id { get; set; }
        public DateTimeOffset CreatedAt { get; }
        public string Mention { get; }
        public IActivity Activity { get; }
        public UserStatus Status { get; }
        public IImmutableSet<ClientType> ActiveClients { get; }
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultAvatarUrl()
        {
            throw new NotImplementedException();
        }

        public async Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string AvatarId { get; }
        public string Discriminator { get; }
        public ushort DiscriminatorValue { get; }
        public bool IsBot { get; }
        public bool IsWebhook { get; }
        public string Username { get; }
    }
}
