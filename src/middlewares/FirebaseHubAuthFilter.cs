using System.Security.Claims;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class FirebaseHubAuthFilter : IHubFilter
{
    private readonly ILogger<FirebaseHubAuthFilter> _logger;

    public FirebaseHubAuthFilter(ILogger<FirebaseHubAuthFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, 
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var methodName = invocationContext.HubMethodName;
        var connectionId = invocationContext.Context.ConnectionId;

        try
        {
            // Skip auth for negotiation and ping methods
            if (methodName == "Negotiate" || methodName == "ping")
            {
                _logger.LogDebug("[AUTH] Skipping auth for method {MethodName}", methodName);
                return await next(invocationContext);
            }

            var token = GetTokenFromContext(invocationContext.Context);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("[AUTH] Missing token for method {MethodName} (Connection: {ConnectionId})", 
                    methodName, connectionId);
                throw new HubException("Authorization token required");
            }

            _logger.LogDebug("[AUTH] Verifying token for method {MethodName} (Connection: {ConnectionId})", 
                methodName, connectionId);

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            var userId = decodedToken.Uid;

            _logger.LogInformation("[AUTH] Authenticated user {UserId} for method {MethodName}", 
                userId, methodName);

            // Attach user claims to context for access in hub methods
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, decodedToken.Claims.GetValueOrDefault("name")?.ToString() ?? "")
            };

            var identity = new ClaimsIdentity(claims, "Firebase");
            var httpContext = invocationContext.Context.GetHttpContext();
            if (httpContext != null)
            {
                httpContext.User = new ClaimsPrincipal(identity);
            }
            else
            {
                _logger.LogWarning("[AUTH] HTTP context is null for connection {ConnectionId}", connectionId);
                throw new HubException("HTTP context is null");
            }

            return await next(invocationContext);
        }
        catch (FirebaseAuthException authEx)
        {
            _logger.LogError(authEx, "[AUTH] Firebase validation failed for method {MethodName}", methodName);
            throw new HubException("Invalid authentication token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AUTH] Unexpected error during authentication for method {MethodName}", methodName);
            throw new HubException("Authentication failed");
        }
    }

    private string? GetTokenFromContext(HubCallerContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            _logger.LogWarning("[AUTH] Missing HTTP context for connection {ConnectionId}", context.ConnectionId);
            return null;
        }

        // Try query string first (WebSocket connections)
        var token = httpContext.Request.Query["access_token"].FirstOrDefault();
        
        // Fall back to Authorization header (other transports)
        if (string.IsNullOrEmpty(token))
        {
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            token = authHeader?.Split(' ').Last(); // Handles "Bearer token" format
        }

        // Log token source for debugging
        if (!string.IsNullOrEmpty(token))
        {
            _logger.LogDebug("[AUTH] Found token from {Source} (Connection: {ConnectionId})",
                httpContext.Request.Query.ContainsKey("access_token") ? "query string" : "header",
                context.ConnectionId);
        }

        return token;
    }
}