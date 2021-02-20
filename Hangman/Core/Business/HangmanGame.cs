using System.Collections.Generic;
using System.Linq;
using Hangman.Core.Models;

namespace Hangman.Core.Business
{
    public class HangmanGame : IHangmanGame
    {
        // utility
        public bool IsGuessedLetterInGuessWord(string guessLetter, string guessWord)
        {
            return guessWord.ToLower().Contains(guessLetter.ToLower());
        }

        public IEnumerable<string> GetGuessWordSoFar(IEnumerable<string> guessedLetters, string guessWord)
        {
            guessedLetters ??= new List<string>() { };

            var guessedLettersSoFar = string.Join("", guessedLetters.ToArray());

            return guessWord.Select(guessWordChar =>
                guessedLettersSoFar.Contains(guessWordChar.ToString()) ? guessWordChar.ToString() : "*").ToList();
        }

        public GameRound ReducePlayerHealth(GameRound gameRound, int amountOfHits = 1)
        {
            gameRound.Health -= amountOfHits;
            return gameRound;
        }

        public GameRound FinishGameRound(GameRound gameRound)
        {
            gameRound.IsOver = true;
            return gameRound;
        }

        public bool HasPlayerBeenHung(GameRound gameRound)
        {
            return gameRound.Health <= 0;
        }

        public bool HasPlayerHasDiscoveredGuessWord(IEnumerable<string> guessedLetters, string guessWord)
        {
            var guessWordSoFar = GetGuessWordSoFar(guessedLetters, guessWord);
            return guessWordSoFar.All(letter => letter != "*");  // no more "*" (not found letters)
        }
    }
}