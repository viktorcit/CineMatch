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
            catch (OperationCanceledException)
                when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Request aborted. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);
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
                OperationCanceledException
                { InnerException: TimeoutException } => StatusCodes.Status504GatewayTimeout,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static BaseResponseDto CreateErrorResponse(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status409Conflict => ResponseFactory.Fail(ErrorType.Conflict, ResponseMessages.Conflict),
                StatusCodes.Status500InternalServerError => ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError),
                StatusCodes.Status503ServiceUnavailable => ResponseFactory.Fail(ErrorType.Timeout, "An upstream service did not respond in time."),
                _ => ResponseFactory.Fail(ErrorType.ServerError, ResponseMessages.ServerError),
            };
        }
    }
}
