using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hangman.Core.Models;

namespace Hangman.Core.Repository.Interfaces
{
    public interface IHangmanRepository<T> where T : BaseEntity
    {
        public T GetById(Guid id);
        public IEnumerable<T> All();
        public IEnumerable<T> Filter(Expression<Func<T, bool>>? filterPredicate = null);
        public void Save(T entity);
        public void Delete(T entity);
        public void Update(T entity);
        public int Count(Expression<Func<T, bool>> predicate);
    }
}