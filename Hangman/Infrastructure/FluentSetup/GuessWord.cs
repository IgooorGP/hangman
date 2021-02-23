using Hangman.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Infrastructure.FluentSetup
{
    public static class FluentGuessWord
    {
        public static void Setup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuessWord>(entity =>
            {
                entity.Property(p => p.Value).IsRequired().HasMaxLength(100);
            });
        }
    }
}