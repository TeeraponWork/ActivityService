using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters
{
    public sealed class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) => _logger = logger;

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled");
            var problem = new ProblemDetails
            {
                Title = "An error occurred",
                Detail = context.Exception.Message,
                Status = StatusCodes.Status400BadRequest
            };
            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
        }
    }
}
