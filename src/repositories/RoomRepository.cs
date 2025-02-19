using Google.Cloud.Firestore;

public class RoomRepository : IRoomRepository
{
    private readonly FirestoreDb _firestoreDb;

    public RoomRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<Room> GetRoomAsync(string roomId)
    {
        var roomRef = _firestoreDb.Collection("Rooms").Document(roomId);
        var snapshot = await roomRef.GetSnapshotAsync();
        return snapshot.Exists ? snapshot.ConvertTo<Room>() : null;
    }

    public async Task CreateRoomAsync(Room room)
    {
        var roomRef = _firestoreDb.Collection("Rooms").Document(room.RoomId);
        await roomRef.SetAsync(room);
    }

    public async Task AddUserToRoomAsync(string roomId, User user)
    {
        var roomRef = _firestoreDb.Collection("Rooms").Document(roomId);
        await roomRef.UpdateAsync($"Users.{user.Id}", user);
    }

    public async Task RemoveUserFromRoomAsync(string roomId, string userId)
    {
        var roomRef = _firestoreDb.Collection("Rooms").Document(roomId);
        await roomRef.UpdateAsync($"Users.{userId}", FieldValue.Delete);
    }

    public async Task AddMessageToRoomAsync(string roomId, Message message)
    {
        var roomRef = _firestoreDb.Collection("Rooms").Document(roomId);
        await roomRef.UpdateAsync("Messages", FieldValue.ArrayUnion(message));
    }

    public async Task<List<string>> GetRoomsAsync()
    {
        var snapshot = await _firestoreDb.Collection("Rooms").GetSnapshotAsync();
        return snapshot.Documents.Select(doc => doc.Id).ToList();
    }
}