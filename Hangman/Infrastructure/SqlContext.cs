using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangman.Core.Models;
using Hangman.Infrastructure.FluentSetup;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Infrastructure
{
    /// <summary>
    /// Application's SQL database context.
    /// </summary>
    public class SqlContext : DbContext
    {
        public SqlContext(DbContextOptions<SqlContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<GameRoom> GameRooms { get; set; } = null!;
        public DbSet<GameRoomUser> GameRoomUsers { get; set; } = null!;
        public DbSet<GuessWord> GuessWords { get; set; } = null!;
        public DbSet<GuessLetter> GuessLetters { get; set; } = null!;

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
        /// Uses EF Fluent API to configure models.
        /// </summary>
        /// <param name="modelBuilder">The model builder to use Fluent API.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            FluentUser.Setup(modelBuilder);
            FluentGameRoom.Setup(modelBuilder);
            FluentGameRoomUser.Setup(modelBuilder);
            FluentGuessWord.Setup(modelBuilder);
            FluentGuessLetter.Setup(modelBuilder);
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