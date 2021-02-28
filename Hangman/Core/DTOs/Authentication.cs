namespace Hangman.Core.DTOs
{
    public class AuthenticationRequestDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoggedUserJwtResponseDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}