
using System.ComponentModel.DataAnnotations;
using CivicSense.DataAccess;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.Web.UI
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            int statusCode;
            string title;
            string detail;

            switch (exception)
            {
                case ValidationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Validation failed";
                    detail = exception.Message;
                    break;

                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Invalid request";
                    detail = "The request contains invalid information.";
                    break;

                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status403Forbidden;
                    title = "Access denied";
                    detail = "You are not allowed to perform this action.";
                    break;

                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Resource not found";
                    detail = "The requested resource was not found.";
                    break;

                case DbUpdateConcurrencyException:
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Concurrency conflict";
                    detail =
                        "This record was changed by another request. " +
                        "Please reload the latest data and try again.";
                    break;

                case DbUpdateException dbException
                    when IsUniqueConstraintViolation(dbException):
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Duplicate record";
                    detail =
                        "A conflicting record already exists. " +
                        "Please refresh and try again.";
                    break;

                default:
                    statusCode =
                        StatusCodes.Status500InternalServerError;
                    title = "Server error";
                    detail =
                        "An unexpected error occurred. " +
                        "Please try again later.";
                    break;
            }

            if (statusCode >= 500)
            {
                _logger.LogError(
                    exception,
                    "Unhandled error. TraceId: {TraceId}",
                    httpContext.TraceIdentifier);
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Request failed. Status: {StatusCode}. TraceId: {TraceId}",
                    statusCode,
                    httpContext.TraceIdentifier);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType =
                "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            problem.Extensions["traceId"] =
                httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(
                problem,
                cancellationToken);

            return true;
        }

        private static bool IsUniqueConstraintViolation(
            DbUpdateException exception)
        {
            return exception.InnerException is SqlException sql &&
                   (sql.Number == 2601 || sql.Number == 2627);
        }
    }
}
