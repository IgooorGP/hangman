using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangman.Models;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Infrastructure
{
    /// <summary>
    /// Application's SQL database context.
    /// </summary>
    public class SqlContext : DbContext
    {
        public SqlContext(DbContextOptions<SqlContext> options) : base(options) { }

        public DbSet<GameRoom> GameRooms { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<GameRoomPlayer> GameRoomPlayers { get; set; } = null!;

        /// <summary>
        /// Override to add CreatedAt and UpdatedAt automatically by using Fluent API.
        /// </summary>
        public override int SaveChanges()
        {
            AutomaticallyAddCreatedAndUpdatedAt();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override to add CreatedAt and UpdatedAt automatically by using Fluent API (async).
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AutomaticallyAddCreatedAndUpdatedAt();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Calls Fluent API to override conventions. Used to provide a customized join-table
        /// for the Player and GameRoom many-to-many relationship.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameRoomPlayer>()
                .HasKey(gameRoomPlayer => new { gameRoomPlayer.GameRoomId, gameRoomPlayer.PlayerId });

            modelBuilder.Entity<GameRoomPlayer>()
                .HasOne(gameRoomPlayer => gameRoomPlayer.Player)
                .WithMany(user => user.GameRoomPlayers)
                .HasForeignKey(gameRoomPlayer => gameRoomPlayer.PlayerId);

            modelBuilder.Entity<GameRoomPlayer>()
                .HasOne(gameRoomPlayer => gameRoomPlayer.GameRoom)
                .WithMany(gameRoom => gameRoom.GameRoomPlayers)
                .HasForeignKey(gameRoomPlayer => gameRoomPlayer.GameRoomId);
        }

        private void AutomaticallyAddCreatedAndUpdatedAt()
        {
            var entitiesOnDbContext = ChangeTracker.Entries<BaseEntity>();

            if (entitiesOnDbContext == null) return; // nothing was changed on DB context

            // createdAt addition
            foreach (var item in entitiesOnDbContext.Where(t => t.State == EntityState.Added))
            {
                item.Entity.CreatedAt = System.DateTime.Now;
                item.Entity.UpdatedAt = System.DateTime.Now;

                if (item.Entity.GetType().Name == "GameRoomPlayer")
                {
                    item.Entity.Id = Guid.NewGuid();
                }
            }

            // updatedAt addition
            foreach (var item in entitiesOnDbContext.Where(t => t.State == EntityState.Modified))
            {
                item.Entity.UpdatedAt = System.DateTime.Now;
            }
        }
    }
}