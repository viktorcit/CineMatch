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
            using var logScope = _logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier });
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
                when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug("Request aborted. Method: {Method}, Path: {Endpoint}, TraceId: {TraceId}",
                    context.Request.Method,
                    context.GetEndpoint()?.DisplayName ?? "Unmatched request",
                    context.TraceIdentifier);
            }
            catch (Exception ex)
            {
                _logger.Log(ex is DbUpdateConcurrencyException ? LogLevel.Warning : LogLevel.Error, ex, "Request failed: {Method} {Endpoint}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.GetEndpoint()?.DisplayName ?? "Unmatched request",
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
