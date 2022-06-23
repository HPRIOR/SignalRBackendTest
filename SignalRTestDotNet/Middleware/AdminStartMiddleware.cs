using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SignalRTestDotNet.GameContextNs;

namespace SignalRTestDotNet.Middleware;

public class AdminStartMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GameContext _gameContext;

    public AdminStartMiddleware(RequestDelegate next, GameContext gameContext)
    {
        _next = next;
        _gameContext = gameContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // New session started on client by deleting 'AdminId' cookie. Hence a new 'AdminId' cookie
        // is generated here only when absent
        if (context.Request.Path == "/hub" && !context.Request.Cookies.ContainsKey("AdminId"))
        {
            var adminId = Guid.NewGuid().ToString();
            var sessionId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("AdminId", adminId);
            context.Response.Cookies.Append("SessionId", sessionId);
            // store in database
            
            var session = new Session{AdminId = adminId, SessionId = sessionId};
            _gameContext.Add(session);
            

        }

        await _next(context);
    }
}
