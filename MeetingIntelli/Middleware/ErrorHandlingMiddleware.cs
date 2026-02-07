

//using MeetingIntelli.DTO.Responses;

//namespace MeetingIntelli.Middleware;

///// <summary>
///// Global error handling middleware for consistent error responses
///// </summary>
//public class ErrorHandlingMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<ErrorHandlingMiddleware> _logger;

//    public ErrorHandlingMiddleware(
//        RequestDelegate next,
//        ILogger<ErrorHandlingMiddleware> logger)
//    {
//        _next = next;
//        _logger = logger;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        try
//        {
//            await _next(context);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unhandled exception occurred");
//            await HandleExceptionAsync(context, ex);
//        }
//    }

//    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
//    {
//        var statusCode = exception switch
//        {
            
//            ArgumentException => HttpStatusCode.BadRequest,
//            InvalidOperationException => HttpStatusCode.BadRequest,
//            KeyNotFoundException => HttpStatusCode.NotFound,
//            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
           
//            _ => HttpStatusCode.InternalServerError
//        };

//        var response = ApiResponse<object>.ErrorResponse(
//            exception.Message,
//            new List<string> { exception.GetType().Name }
//        );

//        context.Response.ContentType = "application/json";
//        context.Response.StatusCode = (int)statusCode;

//        var jsonOptions = new JsonSerializerOptions
//        {
//            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
//        };

//        return context.Response.WriteAsync(
//            JsonSerializer.Serialize(response, jsonOptions)
//        );
//    }
//}