using CineMatch.Api.Helpers;

namespace CineMatch.Api.Midlleware
{
    public class ExeptionHandlingMidlleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExeptionHandlingMidlleware> _logger;

        public ExeptionHandlingMidlleware(RequestDelegate next, ILogger<ExeptionHandlingMidlleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled exception occurred: {ex.Message}");

                if (context.Response.HasStarted)
                    throw;

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    Message = ResponseMessages.ServerError
                });
            }
        }
    }
}
