using CineMatch.Api.Data.DTO.ResponsesDto;
using CineMatch.Api.Enums;
using CineMatch.Api.Helpers;
using Microsoft.EntityFrameworkCore;

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
                _logger.LogError(ex, "Processing error {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                if (context.Response.HasStarted)
                    throw;

                var statusCode = GetStatusCodeFromException(ex);
                context.Response.StatusCode = statusCode;
                var baseResponse = CreateErrorResponse(statusCode);
                await context.Response.WriteAsJsonAsync(baseResponse);
            }
        }


        //private methods
        private static int GetStatusCodeFromException(Exception ex)
        {
            return ex switch
            {
                DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
                DbUpdateException => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static BaseResponseDto CreateErrorResponse(int statusCode)
        {
            switch (statusCode)
            {
                case StatusCodes.Status409Conflict:
                    return ResponseFactory.Fail(ErrorType.Conflict, ResponseMessages.Conflict);
                case StatusCodes.Status500InternalServerError:
                    return ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
                default:
                    return ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError);
            }
        }
    }
}
