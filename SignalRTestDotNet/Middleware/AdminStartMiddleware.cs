using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SignalRTestDotNet.GameContextNs;

namespace SignalRTestDotNet.Middleware;

public class AdminStartMiddleware
{
    private readonly RequestDelegate _next;

    /*
     * Middleware are singletons, and cannot be injected with scoped services
     * Scoped services must be injected to the InvokeAsync method
     */


    public AdminStartMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, GameContext gameContext)
    {
        // New session started on client by deleting 'AdminId' cookie. Hence a new 'AdminId' cookie
        // is generated here only when absent
        if (context.Request.Path == "/hub" && !context.Request.Cookies.ContainsKey("AdminId"))
        {
            var adminId = Guid.NewGuid().ToString();
            var sessionId = Guid.NewGuid().ToString();

            // store in database
            var session = new Session { AdminId = adminId, SessionId = sessionId };
            var sess = await gameContext.Sessions.AddAsync(session);
            await gameContext.SaveChangesAsync();

            // if database save successful return cookies to user
            context.Response.Cookies.Append("AdminId", adminId);
            context.Response.Cookies.Append("SessionId", sessionId);

        }

        await _next(context);
    }
}
