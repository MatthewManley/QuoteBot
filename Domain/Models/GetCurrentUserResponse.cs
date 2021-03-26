namespace Domain.Models
{

    public class GetCurrentUserResponse
    {
        /// <summary>
        /// the user's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// the user's username, not unique across the platform
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// the user's 4-digit discord-tag
        /// </summary>
        public string Discriminator { get; set; }

        /// <summary>
        /// the user's avatar hash
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// whether the user belongs to an OAuth2 application
        /// </summary>
        public bool? Bot { get; set; }

        /// <summary>
        /// whether the user is an Official Discord System user (part of the urgent message system)
        /// </summary>
        public bool? System { get; set; }

        /// <summary>
        /// whether the user has two factor enabled on their account
        /// </summary>
        public bool? MfaEnabled { get; set; }

        /// <summary>
        /// the user's chosen language option
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// whether the email on this account has been verified
        /// </summary>
        public bool? Verified { get; set; }

        /// <summary>
        /// the user's email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// the flags on a user's account
        /// <see cref="https://discord.com/developers/docs/resources/user#user-object-user-flags"/>
        /// </summary>
        public int? Flags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? PremiumType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? PublicFlags { get; set; }
    }
}
