public interface IRoomRepository
{
    Task<FirestoreRoom?> GetRoomAsync(string roomId);
    Task CreateRoomAsync(FirestoreRoom room);
    Task AddUserToRoomAsync(string roomId, FirestoreUser user);
    Task RemoveUserFromRoomAsync(string roomId, string userId);
    Task AddMessageToRoomAsync(string roomId, FirestoreMessage message);
    Task<List<FirestoreRoom>> GetRoomsAsync();
}