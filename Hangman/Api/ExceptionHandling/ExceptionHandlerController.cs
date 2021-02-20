using System.Threading.Tasks;
using Hangman.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Hangman.Api.ExceptionHandling
{
    /// <summary>
    /// The default error controller which sorta works like a lambda exception handler to capture thrown
    /// exceptions and model them accordingly to their final public api response. By default, a custom
    /// ApiErrorResponse model is returned with a message and a status code.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ExceptionHandlerController : ControllerBase
    {
        [Route("handler")]
        public async Task ExceptionHandler()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var capturedException = context.Error;
            var code = 500;

            if (capturedException is HttpStatusException httpException)
                code = (int)httpException.Status;

            // StatusCode of the response
            Response.StatusCode = code;
            Response.ContentType = "application/json";

            await Response.WriteAsync(JsonConvert
                .SerializeObject(new ApiExceptionResponse(capturedException)));
        }
    }
}