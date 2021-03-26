namespace Discord
{
    public static class Mappings
    {

        public static Domain.Models.AccessTokenResponse MapToDomain(this Responses.AccessTokenResponse input)
        {
            return new Domain.Models.AccessTokenResponse
            {
                AccessToken = input.AccessToken,
                TokenType = input.TokenType,
                ExpiresIn = input.ExpiresIn,
                RefreshToken = input.RefreshToken,
                Scope = input.Scope
            };
        }

        public static Domain.Models.GetCurrentUserResponse MapToDomain(this Responses.GetCurrentUserResponse input)
        {
            return new Domain.Models.GetCurrentUserResponse
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

        public static Domain.Models.UserGuild MapToDomain(this Responses.UserGuild input)
        {
            return new Domain.Models.UserGuild
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
    }
}
