using System.Security.Claims;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NanoidDotNet;
using Microsoft.Extensions.Logging;

[Authorize]
public class RoomHub(RoomService roomService, ILogger<RoomHub> logger) : Hub
{
    private readonly RoomService _roomService = roomService;
    private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.DefaultInstance;
    private readonly ILogger<RoomHub> _logger = logger;

    public override async Task OnConnectedAsync()
    {
        try
        {
            var user = Context.User;
            var connectionId = Context.ConnectionId;

            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = user.FindFirst(ClaimTypes.Name)?.Value;

                _logger.LogInformation("[CONNECT] User {Username} ({UserId}) connected with connection {ConnectionId}", username, userId, connectionId);
                await Groups.AddToGroupAsync(connectionId, $"User-{userId}");
            }
            else
            {
                _logger.LogWarning("[CONNECT] Unauthenticated connection attempt: {ConnectionId}", connectionId);
                Context.Abort();
                return;
            }

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CONNECT] Error during connection for {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[DISCONNECT] Unauthenticated user disconnected: {ConnectionId}", connectionId);
                return;
            }

            if (exception != null)
            {
                _logger.LogError(exception, "[DISCONNECT] User {UserId} disconnected with error", userId);
            }
            else
            {
                _logger.LogInformation("[DISCONNECT] User {UserId} disconnected normally", userId);
            }

            foreach (Room room in await _roomService.RemoveUserFromRooms(userId))
            {
                _logger.LogDebug("[CLEANUP] Removing user {UserId} from room {RoomId}", userId, room.RoomId);
                await Clients.Group(room.RoomId).SendAsync("UserRemoved", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DISCONNECT] Error during disconnection for user {UserId}", userId);
            throw;
        }
    }

    public async Task CreateRoom(string roomName, User user)
    {
        try
        {
            string roomId = Nanoid.Generate(size: 8);
            _logger.LogInformation("[ROOM] Creating room {RoomName} with ID {RoomId} for user {UserId}", roomName, roomId, user.Id);
            
            await _roomService.CreateRoomAsync(roomName, roomId, user.Id, user.Username);
            await Clients.Caller.SendAsync("RoomCreated", roomName);
            await AddUserToRoom(roomId, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ROOM] Error creating room {RoomName} for user {UserId}", roomName, user.Id);
            throw;
        }
    }

    public async Task JoinRoom(string roomId)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[JOIN] Unauthenticated join attempt for room {RoomId}", roomId);
                return;
            }

            _logger.LogDebug("[JOIN] User {UserId} joining room {RoomId}", userId, roomId);

            UserRecord userRecord = await _firebaseAuth.GetUserAsync(userId);
            var user = new User
            {
                Id = userRecord.Uid,
                Username = userRecord.DisplayName ?? "Usuário Desconhecido"
            };

            await AddUserToRoom(roomId, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[JOIN] Error joining room {RoomId} for user {UserId}", roomId, userId);
            throw;
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        var userId = Context.UserIdentifier;

        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("[LEAVE] Unauthenticated leave attempt for room {RoomId}", roomId);
                return;
            }

            _logger.LogDebug("[LEAVE] User {UserId} leaving room {RoomId}", userId, roomId);

            UserRecord userRecord = await _firebaseAuth.GetUserAsync(userId);
            var username = userRecord.DisplayName ?? "Usuário Desconhecido";

            await RemoveUserFromRoom(roomId, userId);
            // await _roomService.RemoveUserFromRoomAsync(roomId, userId);
            _logger.LogInformation("[LEAVE] User {Username} ({UserId}) left room {RoomId}", username, userId, roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LEAVE] Error leaving room {RoomId} for user {UserId}", roomId, userId);
            throw;
        }
    }

    public async Task AddUserToRoom(string roomId, User user)
    {
        try
        {
            _logger.LogDebug("[ADD] Adding user {UserId} to room {RoomId}", user.Id, roomId);
            await _roomService.AddUserToRoomAsync(roomId, user.Id, user.Username);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserAdded", user.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ADD] Error adding user {UserId} to room {RoomId}", user.Id, roomId);
            throw;
        }
    }

    public async Task RemoveUserFromRoom(string roomId, string userId)
    {
        try
        {
            _logger.LogDebug("[REMOVE] Removing user {UserId} from room {RoomId}", userId, roomId);
            await _roomService.RemoveUserFromRoomAsync(roomId, userId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserRemoved", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[REMOVE] Error removing user {UserId} from room {RoomId}", userId, roomId);
            throw;
        }
    }

    public async Task SendMessageToRoom(string roomId, string text)
    {
        var userId = Context.UserIdentifier;

        try
        {
            _logger.LogDebug("[MESSAGE] User {UserId} sending message to room {RoomId}", userId, roomId);
            UserRecord userRecord = await _firebaseAuth.GetUserAsync(userId);
            await _roomService.SendMessageToRoomAsync(roomId, userRecord.DisplayName, text);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", userRecord.DisplayName, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MESSAGE] Error sending message to room {RoomId} by user {UserId}", roomId, userId);
            throw;
        }
    }

    public async Task<List<string?>> GetUsersInRoom(string roomId)
    {
        try
        {
            _logger.LogDebug("[USERS] Getting users in room {RoomId}", roomId);
            return await _roomService.GetUsersInRoomAsync(roomId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[USERS] Error getting users in room {RoomId}", roomId);
            throw;
        }
    }

    public async Task<List<Room>> GetRooms()
    {
        try
        {
            _logger.LogDebug("[ROOMS] Getting all rooms");
            return await _roomService.GetRoomsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ROOMS] Error getting rooms");
            throw;
        }
    }
}