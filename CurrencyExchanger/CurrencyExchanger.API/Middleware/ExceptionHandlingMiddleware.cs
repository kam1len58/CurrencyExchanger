using CurrencyExchanger.API.Exceptions;

namespace CurrencyExchanger.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, exception.Message);

        (int code, ProblemDetails problemDetails) = exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Code = 404,
                    Message = exception.Message,
                    Title = "Not found"
                }
            ),

            BadRequestException => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Code = 400,
                    Message = exception.Message,
                    Title = "Bad request"
                }
            ),

            ConflictException => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Code = 409,
                    Message = exception.Message,
                    Title = "Conflict"
                }
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Code = 500,
                    Message = "База данных недоступна",
                    Title = "An unexpected error occurred"
                }
            ),

        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
