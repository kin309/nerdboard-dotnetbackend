using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class FirebaseUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var token = connection.GetHttpContext()?.Request.Query["access_token"].ToString();
        
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var decodedToken = FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token).Result;
                var userId = decodedToken.Uid;

                Console.WriteLine($"[SignalR] UserIdProvider recebeu: {userId}");
                return userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Erro ao verificar token: {ex.Message}");
            }
        }

        return null;
    }
}

public class FirebaseAuthMiddleware
{
    private readonly RequestDelegate _next;

    public FirebaseAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                var uid = decodedToken.Uid;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, uid),
                    new Claim(ClaimTypes.Name, decodedToken.Claims["name"]?.ToString() ?? "")
                };

                var identity = new ClaimsIdentity(claims, "Firebase");
                context.User = new ClaimsPrincipal(identity);

                Console.WriteLine($"[Middleware] Usuário autenticado: {uid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Middleware] Erro na autenticação: {ex.Message}");
            }
        }

        await _next(context);
    }
}

