using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class FirebaseUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Uses the already-authenticated user from Context.User
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}