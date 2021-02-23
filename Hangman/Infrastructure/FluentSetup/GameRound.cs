using Hangman.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Infrastructure.FluentSetup
{
    public static class FluentGuessLetter
    {
        public static void Setup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuessLetter>(entity =>
            {
                entity.Property(p => p.Value).HasMaxLength(1);

                // each letter AND guessWord must be UNIQUE
                entity.HasIndex(p => new { p.Value, p.GuessWordId }).IsUnique();
            });
        }
    }
}