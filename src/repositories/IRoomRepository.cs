public interface IRoomRepository
{
    Room? GetRoom(string roomId);
    void CreateRoom(Room room);
    void DeleteRoom(string roomId);
    void AddUserToRoom(string roomId, User user);
    void RemoveUserFromRoom(string roomId, string userId);
    void AddMessageToRoom(string roomId, Message message);
    List<Room> GetAllRooms();
}