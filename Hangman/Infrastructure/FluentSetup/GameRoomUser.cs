using Hangman.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Infrastructure.FluentSetup
{
    public static class FluentGameRoomUser
    {
        public static void Setup(ModelBuilder modelBuilder)
        {
            // composite primary key
            modelBuilder.Entity<GameRoomUser>()
               .HasKey(gameRoomUser => new { gameRoomUser.GameRoomId, gameRoomUser.UserId });

            // one-to-many with User
            modelBuilder.Entity<GameRoomUser>()
                .HasOne(gameRoomUser => gameRoomUser.User)
                .WithMany(user => user.GameRoomUsers)
                .HasForeignKey(gameRoomUser => gameRoomUser.UserId);

            // one-to-many with GameRoom
            modelBuilder.Entity<GameRoomUser>()
                .HasOne(gameRoomUser => gameRoomUser.GameRoom)
                .WithMany(gameRoom => gameRoom.GameRoomUsers)
                .HasForeignKey(gameRoomUser => gameRoomUser.GameRoomId);
        }
    }
}