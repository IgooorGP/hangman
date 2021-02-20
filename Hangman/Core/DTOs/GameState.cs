using System.Collections.Generic;

namespace Hangman.Core.DTOs
{
    public class GameStateDTO
    {
        public string? GuessWord;
        public IEnumerable<string> GuessWordSoFar = null!;
        public IEnumerable<string> GuessedLetters = null!;
        public bool IsOver;
        public int PlayerHealth;
    }

}