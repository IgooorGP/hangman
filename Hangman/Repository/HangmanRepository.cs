using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Hangman.Infrastructure;
using Hangman.Models;
using Hangman.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Repository
{
    /**
    * Django-like model implementation of a repository.
    */
    public class HangmanRepository<T> : IHangmanRepository<T> where T : BaseEntity
    {
        private readonly SqlContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public HangmanRepository(SqlContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public T GetById(Guid id) => _dbSet.SingleOrDefault(e => e.Id == id);

        public IEnumerable<T> All() => _dbSet.ToList();

        public IEnumerable<T> Filter(Expression<Func<T, bool>>? filterPredicate = null)
        {
            var query = _dbSet.AsQueryable();

            if (filterPredicate != null)
            {
                query = query.Where(filterPredicate);
            }

            return query.ToList();
        }

        public void Save(T entity)
        {
            _dbSet.Add(entity);
            _dbContext.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
            _dbContext.SaveChanges();
        }

        public void Update(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return predicate == null ? _dbSet.Count() : _dbSet.Count(predicate);
        }
    }
}