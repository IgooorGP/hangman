namespace Hangman
{
    public class SecretsConfig
    {
        public string JwtSignaturePrivateKey { get; set; } = string.Empty;
        public int JwtExpirationPeriodInHours { get; set; } = 1;
    }
}