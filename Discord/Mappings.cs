using System;

namespace Discord
{
    public static class Mappings
    {

        public static Domain.Models.Discord.AccessTokenResponse MapToDomain(this Responses.AccessTokenResponse input)
        {
            return new Domain.Models.Discord.AccessTokenResponse
            {
                AccessToken = input.AccessToken,
                TokenType = input.TokenType,
                ExpiresIn = input.ExpiresIn,
                RefreshToken = input.RefreshToken,
                Scope = input.Scope
            };
        }

        public static Domain.Models.Discord.User MapToDomain(this Responses.GetCurrentUserResponse input)
        {
            return new Domain.Models.Discord.User
            {
                Id = input.Id,
                Username = input.Username,
                Discriminator = input.Discriminator,
                Avatar = input.Avatar,
                Bot = input.Bot,
                System = input.System,
                MfaEnabled = input.MfaEnabled,
                Locale = input.Locale,
                Verified = input.Verified,
                Email = input.Email,
                Flags = input.Flags,
                PremiumType = input.PremiumType,
                PublicFlags = input.PublicFlags
            };
        }

        public static Domain.Models.Discord.UserGuild MapToDomain(this Responses.UserGuild input)
        {
            return new Domain.Models.Discord.UserGuild
            {
                IdString = input.IdString,
                Name = input.Name,
                Icon = input.Icon,
                Owner = input.Owner,
                PermissionsString = input.PermissionsString,
                PermissionsInt = input.PermissionsInt,
                Features = input.Features
            };
        }

        public static Domain.Models.Discord.GuildRole MapToDomain(this Responses.GuildRole input)
        {
            return new Domain.Models.Discord.GuildRole
            {
                Id = ulong.Parse(input.Id),
                Name = input.Name,
                Color = input.Color,
                Hoist = input.Hoist,
                Position = input.Position,
                Permissions = input.Permissions,
                Managed = input.Managed,
                Mentionable = input.Mentionable
            };
        }

        public static Domain.Models.Discord.GuildChannel MapToDomain(this Responses.GuildChannel input)
        {
            var hasGuildId = ulong.TryParse(input.GuildId, out var guildId);
            return new Domain.Models.Discord.GuildChannel
            {
                Id = ulong.Parse(input.Id),
                ChannelType = (Domain.Models.Discord.GuildChannelType)input.ChannelType,
                GuildId = hasGuildId ? guildId : null,
                Name = input.Name,
                Position = input.Position
            };
        }
    }
}
