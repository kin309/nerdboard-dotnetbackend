using Microsoft.AspNetCore.Identity;
using NanoidDotNet;

public class RoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public Room CreateRoom(string roomName, string userId, string userName)
    {
        string roomId = Nanoid.Generate(size: 8);
        var room = new Room
        {
            Id = roomId,
            Name = roomName,
            CreatedBy = userName
        };
        _roomRepository.CreateRoom(room);
        return room;
    }

    public void DeleteRoom(string roomId)
    {
        _roomRepository.DeleteRoom(roomId);
    }

    public void AddUserToRoom(string roomId, string userId, string username)
    {
        var user = new User { Id = userId, Username = username };
        _roomRepository.AddUserToRoom(roomId, user);
    }

    public void RemoveUserFromRoom(string roomId, string userId)
    {
        _roomRepository.RemoveUserFromRoom(roomId, userId);
        if (GetRoom(roomId)?.Users.Count == 0){
            _roomRepository.DeleteRoom(roomId); // Remove the room if the user is the last one in it
        }
    }

    public void SendMessageToRoom(string roomId, string userName, string text)
    {
        var message = new Message { SenderId = userName, Content = text };
        _roomRepository.AddMessageToRoom(roomId, message);
    }

    public List<User> GetUsersInRoom(string roomId)
    {
        var room = _roomRepository.GetRoom(roomId);
        return room?.Users.Values.Select(u => u).ToList() ?? new List<User>();
    }

    public List<Room> GetRooms()
    {
        List<Room> rooms = _roomRepository.GetAllRooms();
        return rooms;
    }

    public List<Room> GetRoomsByUserIdAsync(string userId)
    {  
        List<Room> rooms = _roomRepository.GetAllRooms(); // Busca todas as salas
        var roomsWithUser = new List<Room>(); // Lista para armazenar as salas que contêm o usuário
        foreach (var room in rooms)
        {
            foreach(var user in room.Users)
            {
                // Verifica se o usuário está na sala
                if (user.Value.Id == userId)
                {
                    roomsWithUser.Add(room);
                    break;
                }
            }
        }

        return roomsWithUser;
    }

    public List<Room> RemoveUserFromRooms(string userId)
    {
        // 🔹 Remove o usuário de todas as salas
        var rooms = GetRoomsByUserIdAsync(userId);
        foreach (var room in rooms)
        {
            RemoveUserFromRoom(room.Id, userId);
        }

        return rooms;
    }

    public Room GetRoom(string roomId){
        return _roomRepository.GetRoom(roomId);
    }
}