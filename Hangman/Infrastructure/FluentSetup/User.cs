using Hangman.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Infrastructure.FluentSetup
{
    public static class FluentUser
    {
        public static void Setup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(p => p.Username).IsRequired().HasMaxLength(100);
                entity.HasIndex(p => p.Username).IsUnique();
                entity.Property(p => p.Password).IsRequired().HasMaxLength(20);
                entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Role).IsRequired().HasMaxLength(50);
            });
        }
    }
}