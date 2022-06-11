using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SignalRTestDotNet.Middleware;

public class AdminStartMiddleware
{
    private readonly RequestDelegate _next;

    public AdminStartMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // New session started on client by deleting 'AdminId' cookie. Hence a new 'AdminId' cookie
        // is generated here only when absent
        if (context.Request.Path == "/hub" && !context.Request.Cookies.ContainsKey("AdminId"))
        {
            context.Response.Cookies.Append("AdminId", Guid.NewGuid().ToString());
            context.Response.Cookies.Append("SessionId", Guid.NewGuid().ToString());
            // store in database
        }

        await _next(context);
    }
}
