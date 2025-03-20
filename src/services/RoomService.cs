public class RoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task CreateRoomAsync(string roomName, string roomId, string userId, string userName)
    {
        var room = new FirestoreRoom
        {
            RoomId = roomId,
            RoomName = roomName,
            CreatedBy = userName
        };
        await _roomRepository.CreateRoomAsync(room);
    }

    public async Task AddUserToRoomAsync(string roomId, string userId, string userName)
    {
        var user = new FirestoreUser { Id = userId, Username = userName };
        await _roomRepository.AddUserToRoomAsync(roomId, user);
    }

    public async Task RemoveUserFromRoomAsync(string roomId, string userId)
    {
        await _roomRepository.RemoveUserFromRoomAsync(roomId, userId);
    }

    public async Task SendMessageToRoomAsync(string roomId, string userName, string text)
    {
        var message = new FirestoreMessage { SenderId = userName, Content = text };
        await _roomRepository.AddMessageToRoomAsync(roomId, message);
    }

    public async Task<List<string?>> GetUsersInRoomAsync(string roomId)
    {
        var room = await _roomRepository.GetRoomAsync(roomId);
        return room?.Users.Values.Select(u => u.Username).ToList() ?? new List<string?>();
    }

    public async Task<List<Room>> GetRoomsAsync()
    {
        List<FirestoreRoom> firestoreRooms = await _roomRepository.GetRoomsAsync();

        List<Room> rooms = new List<Room>();
        foreach (var firestoreRoom in firestoreRooms)
        {
            var room = Room.GetRoomFromFirestore(firestoreRoom);
            rooms.Add(room);
        }

        return rooms;
    }

    public async Task<List<Room>> GetRoomsByUserIdAsync(string userId)
    {
        // Busca as associações de salas para o usuário no Firestore
        var userRoomsQuery = await _roomRepository.GetRoomsAsync(); // Busca todas as salas

        List<Room> rooms = new List<Room>();

        foreach (var firestoreRoom in userRoomsQuery)
        {
            // Verifica se o usuário está na sala
            var userInRoom = await _roomRepository.GetUserInRoomAsync(firestoreRoom.RoomId, userId);
            if (userInRoom)
            {
                // Se o usuário está na sala, converte e adiciona à lista de salas
                var room = Room.GetRoomFromFirestore(firestoreRoom);
                rooms.Add(room);
            }
        }

        return rooms;
    }

    public async Task<List<Room>> RemoveUserFromRooms(string userId)
    {
        // 🔹 Remove o usuário de todas as salas
        var rooms = await GetRoomsByUserIdAsync(userId);
        foreach (var room in rooms)
        {
            await RemoveUserFromRoomAsync(room.RoomId, userId);
        }

        return rooms;
    }
}