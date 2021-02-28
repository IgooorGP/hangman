using System;
using System.Net;

namespace Hangman.Core.Exceptions
{
    public class HttpStatusException : Exception
    {
        public HttpStatusCode Status { get; private set; }

        public HttpStatusException(string msg, HttpStatusCode status) : base(msg)
        {
            Status = status;
        }
    }

    public class ObjectDoesNotExist : HttpStatusException
    {
        public ObjectDoesNotExist(string msg) : base(msg, HttpStatusCode.NotFound) { }
    }

    public class ObjectAlreadyExists : HttpStatusException
    {
        public ObjectAlreadyExists(string msg) : base(msg, HttpStatusCode.BadRequest) { }
    }

    public class AuthenticationFailure : HttpStatusException
    {
        public AuthenticationFailure(string msg) : base(msg, HttpStatusCode.BadRequest) { }
    }
}