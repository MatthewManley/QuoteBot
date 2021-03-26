namespace Domain.Models
{
    public class AccessTokenResponse
    {
        public string AccessToken { get; init; }

        public string TokenType { get; init; }

        public int ExpiresIn { get; init; }

        public string RefreshToken { get; init; }

        public string Scope
        {
            get
            {
                return _scope;
            }
            init
            {
                Scopes = value.Split(' ');
                _scope = value;
            }
        }

        private string _scope { set; get; }

        public string[] Scopes { get; private set; }
    }
}
