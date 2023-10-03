using OpenQA.Selenium.DevTools.V117.Network;
using Streamer.Exceptions;

namespace Streamer.Middlewares;

public class GlobalErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BaseResponseException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { Message = ex.Message });
        }
    }
}