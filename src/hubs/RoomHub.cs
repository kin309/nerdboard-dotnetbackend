using Microsoft.AspNetCore.SignalR;
using NanoidDotNet;

public class RoomHub : Hub
{
    private readonly RoomService _roomService;

    public RoomHub(RoomService roomService)
    {
        _roomService = roomService;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Usuário conectado: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Usuário desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task CreateRoom(string roomName, User user)
    {
        Console.WriteLine("Room Created!");
        string roomId = Nanoid.Generate(size:8);
        Console.WriteLine($"Room ID: {roomId}");
        await _roomService.CreateRoomAsync(roomName, roomId, user.Id, user.Username);
        await Clients.Caller.SendAsync("RoomCreated", roomName);
        await AddUserToRoom(roomId, user);
    }

    public async Task AddUserToRoom(string roomId, User user)
    {
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

    public async Task SendMessageToRoom(string roomId, string userName, string text)
    {
        await _roomService.SendMessageToRoomAsync(roomId, userName, text);
        await Clients.Group(roomId).SendAsync("ReceiveMessage", userName, text);
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