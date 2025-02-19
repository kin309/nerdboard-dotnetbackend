public class RoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task CreateRoomAsync(string roomName, string roomId, string userId, string userName)
    {
        var room = new Room
        {
            RoomId = roomId,
            RoomName = roomName,
            CreatedBy = userName
        };
        await _roomRepository.CreateRoomAsync(room);
    }

    public async Task AddUserToRoomAsync(string roomId, string userId, string userName)
    {
        var user = new User { Id = userId, Username = userName };
        await _roomRepository.AddUserToRoomAsync(roomId, user);
    }

    public async Task RemoveUserFromRoomAsync(string roomId, string userId)
    {
        await _roomRepository.RemoveUserFromRoomAsync(roomId, userId);
    }

    public async Task SendMessageToRoomAsync(string roomId, string userName, string text)
    {
        var message = new Message { SenderId = userName, Content = text };
        await _roomRepository.AddMessageToRoomAsync(roomId, message);
    }

    public async Task<List<string>> GetUsersInRoomAsync(string roomId)
    {
        var room = await _roomRepository.GetRoomAsync(roomId);
        return room?.Users.Values.Select(u => u.Username).ToList() ?? new List<string>();
    }

    public async Task<List<string>> GetRoomsAsync()
    {
        return await _roomRepository.GetRoomsAsync();
    }
}