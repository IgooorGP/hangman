using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Repository.Interfaces
{
    public interface IHangmanRepositoryAsync<T> where T : BaseEntity
    {
        ValueTask<T?> GetById(Guid id);
        ValueTask<T?> GetById(Guid id, IEnumerable<string> includes);
        ValueTask<T?> Get(Expression<Func<T, bool>> filterPredicate);
        Task<IEnumerable<T>> All();
        Task<IEnumerable<T>> All(IEnumerable<string> includes);
        ValueTask<IEnumerable<T>> Filter(Expression<Func<T, bool>>? filterPredicate = null);
        Task Save(T entity);
        Task Delete(T entity);
        Task Update(T entity);
        Task<int> Count(Expression<Func<T, bool>> predicate);
    }
}