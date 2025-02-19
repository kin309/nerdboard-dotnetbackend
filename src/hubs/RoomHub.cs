using Microsoft.AspNetCore.SignalR;

public class RoomHub : Hub
{
    private readonly RoomService _roomService;

    public RoomHub(RoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task CreateRoom(string roomName, string roomId, string userId, string userName)
    {
        Console.WriteLine("Room Created!");
        await _roomService.CreateRoomAsync(roomName, roomId, userId, userName);
        await Clients.Caller.SendAsync("RoomCreated", roomName);
    }

    public async Task AddUserToRoom(string roomId, string userId, string userName)
    {
        await _roomService.AddUserToRoomAsync(roomId, userId, userName);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserAdded", userName);
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

    public async Task<List<string>> GetUsersInRoom(string roomId)
    {
        return await _roomService.GetUsersInRoomAsync(roomId);
    }

    public async Task<List<string>> GetRooms()
    {
        return await _roomService.GetRoomsAsync();
    }
}