using System.Collections.Generic;
using Hangman.Core.Models;

namespace Hangman.Core.Business
{
    public interface IHangmanGame
    {
        // utility
        public bool IsGuessedLetterInGuessWord(string guessLetter, string guessWord);
        public IEnumerable<string> GetGuessWordSoFar(IEnumerable<string> guessedLetters, string guessWord);
        public GameRound FinishGameRound(GameRound gameRound);

        // when player guesses a wrong 
        public GameRound ReducePlayerHealth(GameRound gameRound, int amountOfHits = 1);
        public bool HasPlayerBeenHung(GameRound gameRound);

        // when player guesses a correct letter
        public bool HasPlayerHasDiscoveredGuessWord(IEnumerable<string> guessedLetters, string guessWord);
    }
}