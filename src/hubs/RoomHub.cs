using System.Security.Claims;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NanoidDotNet;

[Authorize]
public class RoomHub : Hub
{
    private readonly RoomService _roomService;
    private readonly FirebaseAuth _firebaseAuth;

    public RoomHub(RoomService roomService)
    {
        _roomService = roomService;
        _firebaseAuth = FirebaseAuth.DefaultInstance;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User;

        if (user?.Identity?.IsAuthenticated ?? false)
        {
            // Pega um claim específico, por exemplo, o ID do usuário
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;

            Console.WriteLine(username);
            // var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            
            // Faça algo com os claims, como adicionar o usuário a um grupo
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");
        }

        // Console.WriteLine($"[SignalR] Usuário conectado: {userId}");
    
        // if (string.IsNullOrEmpty(userId))
        // {
        //     Console.WriteLine("[SignalR] Conexão sem autenticação!");
        //     Context.Abort();
        // }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("[SignalR] Usuário desconectado sem autenticação.");
            return;
        }

        Console.WriteLine($"[SignalR] Usuário desconectado: {userId}");

        foreach (Room room in await _roomService.RemoveUserFromRooms(userId)){
            await Clients.Group(room.RoomId).SendAsync("UserRemoved", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task CreateRoom(string roomName, User user)
    {
        Console.WriteLine("Room Created!");
        string roomId = Nanoid.Generate(size: 8);
        Console.WriteLine($"Room ID: {roomId}");
        await _roomService.CreateRoomAsync(roomName, roomId, user.Id, user.Username);
        await Clients.Caller.SendAsync("RoomCreated", roomName);
        await AddUserToRoom(roomId, user);
    }

    public async Task JoinRoom(string roomId)
    {
        Console.WriteLine("JoiningRoom");
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("[SignalR] Tentativa de conexão sem autenticação!");
            return;
        }

        Console.WriteLine($"[SignalR] Usuário autenticado: {userId}");

        UserRecord userRecord;
        try
        {
            userRecord = await _firebaseAuth.GetUserAsync(userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Erro ao buscar usuário: {ex.Message}");
            return;
        }

        var user = new User
        {
            Id = userRecord.Uid,
            Username = userRecord.DisplayName ?? "Usuário Desconhecido"
        };

        await AddUserToRoom(roomId, user);
    }

    public async Task LeaveRoom(string roomId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("[SignalR] Tentativa de saída sem autenticação!");
            return;
        }

        Console.WriteLine($"[SignalR] Usuário saindo: {userId}");

        UserRecord userRecord;
        try
        {
            userRecord = await _firebaseAuth.GetUserAsync(userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalR] Erro ao buscar usuário: {ex.Message}");
            return;
        }

        var username = userRecord.DisplayName ?? "Usuário Desconhecido";

        await _roomService.RemoveUserFromRoomAsync(roomId, userId);

        Console.WriteLine($"[SignalR] Usuário {username} ({userId}) saiu da sala {roomId}");
    }

    public async Task AddUserToRoom(string roomId, User user)
    {
        Console.WriteLine($"Usuário {user.Id} adicionado a sala: {roomId}");
        await _roomService.AddUserToRoomAsync(roomId, user.Id, user.Username);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserAdded", user.Username);
    }

    public async Task RemoveUserFromRoom(string roomId, string userId)
    {
        await _roomService.RemoveUserFromRoomAsync(roomId, userId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserRemoved", userId);
    }

    public async Task SendMessageToRoom(string roomId, string text)
    {
        var userId = Context.UserIdentifier;
        UserRecord userRecord = await _firebaseAuth.GetUserAsync(userId);
        await _roomService.SendMessageToRoomAsync(roomId, userRecord.DisplayName, text);
        await Clients.Group(roomId).SendAsync("ReceiveMessage", userRecord.DisplayName, text);
    }

    public async Task<List<string?>> GetUsersInRoom(string roomId)
    {
        return await _roomService.GetUsersInRoomAsync(roomId);
    }

    public async Task<List<Room>> GetRooms()
    {
        return await _roomService.GetRoomsAsync();
    }
}