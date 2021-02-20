using System;

namespace Hangman.Api.ExceptionHandling
{
    /// <summary>
    /// Base exception model class to return to the API users.
    /// </summary>
    public class ApiExceptionResponse
    {
        public string Message { get; set; }

        public ApiExceptionResponse(Exception ex)
        {
            Message = ex.Message;
        }
    }
}