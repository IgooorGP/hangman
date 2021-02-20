using System.Linq;
using Microsoft.EntityFrameworkCore.Query;

namespace Hangman.Core.Infrastructure
{
    /// <summary>
    /// Extension used to skip database items from the real database query in order to paginate results.
    /// </summary>
    public static class PaginationExtension
    {
        public static IQueryable<T> Paginate<T>(this IOrderedQueryable<T> query, int pageSize, int pageNumber)
        {
            var paginationParams = new ValidPaginationParams(pageSize, pageNumber);

            return query.Skip(paginationParams.ItemsToSkip).Take(paginationParams.PageSize);
        }

        /// <summary>
        /// Nested class that represents pagination parameters after validation.
        /// </summary>
        public class ValidPaginationParams
        {
            // Constraints for pagination: max, min and default values
            private const int MAX_PAGE_SIZE = 50;
            private const int MIN_PAGE_SIZE = 1;
            private const int MIN_PAGE_NUMBER = 1;
            private const int DEFAULT_PAGE_SIZE = 10;
            private const int DEFAULT_PAGE_NUMBER = 1;
            private int _pageSize = DEFAULT_PAGE_SIZE;
            private int _pageNumber = DEFAULT_PAGE_NUMBER;

            /// <summary>
            /// Checks if the page size contains a valid number and sets it accordingly.
            /// </summary>
            /// <value>A valid page size.</value>
            public int PageSize
            {
                get
                {
                    return _pageSize;
                }
                set
                {
                    if (value >= MAX_PAGE_SIZE)
                    {
                        _pageSize = MAX_PAGE_SIZE;
                    }
                    else if (value < MIN_PAGE_SIZE)
                    {
                        _pageSize = MIN_PAGE_SIZE;
                    }
                    else
                    {
                        _pageSize = value;
                    }
                }
            }

            /// <summary>
            /// Checks if the page number contains a valid number and sets it accordingly.
            /// </summary>
            /// <value>A valid page number.</value>
            public int PageNumber
            {
                get
                {
                    return _pageNumber;
                }
                set
                {
                    if (value < MIN_PAGE_NUMBER)
                    {
                        _pageNumber = DEFAULT_PAGE_NUMBER;
                    }
                    else
                    {
                        _pageNumber = value;
                    }
                }
            }
            public int ItemsToSkip
            {
                get { return (PageNumber - 1) * PageSize; }
            }

            /// <summary>
            /// Default constructor to validate pagination parameters. If not valid, uses default values.
            /// </summary>
            /// <param name="pageSize">The desired page size that will undergo validation.</param>
            /// <param name="pageNumber">The desired page number that will undergo validation.</param>        
            public ValidPaginationParams(int pageSize, int pageNumber)
            {
                PageSize = pageSize;
                PageNumber = pageNumber;
            }
        }
    }
}