using Hangman.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Infrastructure.FluentSetup
{
    public static class FluentGameRoom
    {
        public static void Setup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameRoom>(entity =>
            {
                entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
                entity.HasIndex(p => p.Name).IsUnique();
            });
        }
    }
}