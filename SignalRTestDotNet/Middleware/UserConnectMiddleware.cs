using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SignalRTestDotNet.GameContextNs;

namespace SignalRTestDotNet.Middleware;

public class UserConnectMiddleware
{
    private readonly RequestDelegate _next;

    /*
     * Middleware are singletons, and cannot be injected with scoped services
     * Scoped services must be injected to the InvokeAsync method
     */


    public UserConnectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private async Task AddUserClaims(string userId, HttpContext context)
    {
        var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "CookieAuth");
        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
        await context.SignInAsync("CookieAuth", claimsPrincipal);
    }

    /* New admin session started by deleting 'AdminId' cookie on client side, 
     * It is only generated here when absent.
     * All connections are given a UserId if not present so that connections can be persisted 
     * between browser sessions.
     */
    public async Task InvokeAsync(HttpContext context, GameContext gameContext)
    {
        var userId = Guid.NewGuid().ToString();
        if (context.Request.Path == "/hub"
                && !context.Request.Cookies.ContainsKey("AdminId")
                && !context.Request.Cookies.ContainsKey("PlayerId"))
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

            await AddUserClaims(userId,context);

        }

        if (context.Request.Cookies.ContainsKey("PlayerId")
                && !context.Request.Cookies.ContainsKey("CookieAuth"))
        {
            await AddUserClaims(userId, context);

        }

        await _next(context);
    }
}
