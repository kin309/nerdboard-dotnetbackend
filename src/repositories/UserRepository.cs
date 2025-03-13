using Google.Cloud.Firestore;

public class UserRepository : IUserRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly CollectionReference _usersRef;

    public UserRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
        _usersRef = _firestoreDb.Collection("UsersOnline");
    }

    public async Task AddUserAsync(FirestoreUser user)
    {
        await _usersRef.Document(user.Id).SetAsync(user);
    }

    public async Task RemoveUserAsync(string userId)
    {
        await _usersRef.Document(userId).DeleteAsync();
    }

    public async Task<List<FirestoreUser>> GetOnlineUsersAsync()
    {
        var snapshot = await _usersRef.GetSnapshotAsync();
        return snapshot.Documents
            .Select(doc => new FirestoreUser
            {
                Id = doc.Id,
                Username = doc.GetValue<string>("Username"),
                Email = doc.GetValue<string>("Email")
            })
            .ToList();
    }
}