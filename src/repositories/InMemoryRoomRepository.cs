using System.Collections.Concurrent;

public class InMemoryRoomRepository : IRoomRepository
{
    // Armazena as salas em um ConcurrentDictionary para thread-safety
    private readonly ConcurrentDictionary<string, Room> _rooms = new();

    // Obtém uma sala por ID (retorna null se não existir)
    public Room? GetRoom(string roomId)
    {
        _rooms.TryGetValue(roomId, out var room);
        return room;
    }

    // Verifica se um usuário está na sala
    public bool IsUserInRoom(string roomId, string userId)
    {
        return _rooms.TryGetValue(roomId, out var room) &&
               room.Users?.ContainsKey(userId) == true;
    }

    // Cria uma nova sala
    public void CreateRoom(Room room)
    {
        _rooms.TryAdd(room.Id, room);
    }

    // Adiciona um usuário à sala
    public void AddUserToRoom(string roomId, User user)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.Users ??= new Dictionary<string, User>();
            room.Users[user.Id] = user;
            Console.WriteLine("USERNAME >>>>>>>>>>>>> " + room.Users[user.Id].Username);
        }
    }

    // Remove um usuário da sala
    public void RemoveUserFromRoom(string roomId, string userId)
    {
        if (_rooms.TryGetValue(roomId, out var room) &&
            room.Users?.ContainsKey(userId) == true)
        {
            room.Users.Remove(userId);
        }
    }

    // Adiciona uma mensagem à sala
    public void AddMessageToRoom(string roomId, Message message)
    {
        if (_rooms.TryGetValue(roomId, out var room))
        {
            room.Messages ??= new List<Message>();
            room.Messages.Add(message);
        }
    }

    // Retorna todas as salas (lista não-nullable)
    public List<Room> GetAllRooms()
    {
        return _rooms.Values.ToList();
    }

    public void DeleteRoom(string roomId)
    {
        _rooms.TryRemove(roomId, out _);
    }
}