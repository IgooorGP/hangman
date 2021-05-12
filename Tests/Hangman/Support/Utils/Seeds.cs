using System.Threading.Tasks;
using Bogus;
using Hangman.Core.Models;
using Hangman.Infrastructure;

namespace Tests.Hangman.Support.Utils
{
    public static class GameRoomSeeds
    {
        public async static Task<GameRoomUser> GameRoomWithHost(GameRoom gameRoom, User user, SqlContext db)
        {
            var gameRoomUser = new GameRoomUser
            {
                GameRoom = gameRoom,
                GameRoomId = gameRoom.Id,
                User = user,
                UserId = user.Id,
                IsHost = true,
                IsInRoom = true
            };

            await db.GameRooms.AddAsync(gameRoom);
            await db.Users.AddAsync(user);
            await db.GameRoomUsers.AddAsync(gameRoomUser);
            await db.SaveChangesAsync();

            return gameRoomUser;
        }
    }

    public static class UserSeeds
    {
        public async static Task<User> User(User user, SqlContext db)
        {
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return user;
        }
    }
}