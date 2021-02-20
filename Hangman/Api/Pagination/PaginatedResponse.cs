using System.Collections.Generic;

namespace Hangman.Api.Pagination
{
    class PaginatedResponse
    {
        public object Results;
        public int ResultsCount;
        public int Total;

        public PaginatedResponse(object results, int resultsCount, int total)
        {
            Results = results;
            ResultsCount = resultsCount;
            Total = total;
        }
    }
}