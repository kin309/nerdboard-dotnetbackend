public interface IRoomRepository
{
    Task<Room> GetRoomAsync(string roomId);
    Task CreateRoomAsync(Room room);
    Task AddUserToRoomAsync(string roomId, User user);
    Task RemoveUserFromRoomAsync(string roomId, string userId);
    Task AddMessageToRoomAsync(string roomId, Message message);
    Task<List<string>> GetRoomsAsync();
}