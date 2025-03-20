using Google.Cloud.Firestore;

public class RoomRepository : IRoomRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly CollectionReference _roomsRef;

    public RoomRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
        _roomsRef = _firestoreDb.Collection("Rooms");
    }

    public async Task<FirestoreRoom?> GetRoomAsync(string roomId)
    {
        var snapshot = await _roomsRef.Document(roomId).GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<FirestoreRoom>() : null;
    }

    public async Task CreateRoomAsync(FirestoreRoom room)
    {
        await _roomsRef.Document(room.RoomId).SetAsync(room);
    }

    public async Task AddUserToRoomAsync(string roomId, FirestoreUser user)
    {
        await _roomsRef.Document(roomId).UpdateAsync($"Users.{user.Id}", user);
    }

    public async Task RemoveUserFromRoomAsync(string roomId, string userId)
    {
        await _roomsRef.Document(roomId).UpdateAsync($"Users.{userId}", FieldValue.Delete);
    }

    public async Task AddMessageToRoomAsync(string roomId, FirestoreMessage message)
    {
        await _roomsRef.Document(roomId).UpdateAsync("Messages", FieldValue.ArrayUnion(message));
    }

    public async Task<List<FirestoreRoom>> GetRoomsAsync()
    {
        var snapshot = await _firestoreDb.Collection("Rooms").GetSnapshotAsync();

        List<FirestoreRoom> rooms = new List<FirestoreRoom>();

        foreach (var doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                FirestoreRoom room = doc.ConvertTo<FirestoreRoom>();
                rooms.Add(room);
            }
        }

        return rooms;
    }

    public async Task<bool> GetUserInRoomAsync(string roomId, string userId)
    {
        var roomDoc = await _firestoreDb.Collection("rooms").Document(roomId).GetSnapshotAsync();

        if (!roomDoc.Exists)
            return false;

        // Aqui assumimos que você tem uma subcoleção "users" dentro de cada sala, onde cada documento tem o userId
        var usersInRoom = await _firestoreDb.Collection("rooms")
                                             .Document(roomId)
                                             .Collection("users")
                                             .WhereEqualTo("userId", userId)
                                             .GetSnapshotAsync();

        return usersInRoom.Documents.Any();  // Se encontrar algum documento, significa que o usuário está na sala
    }
}