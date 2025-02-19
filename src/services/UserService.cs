using Google.Cloud.Firestore;

public class UserService
{
    private readonly FirestoreDb _firestoreDb;
    private readonly CollectionReference _usersRef;

    public UserService(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
        _usersRef = _firestoreDb.Collection("UsersOnline");
    }

    public async Task AddUserAsync(string userId, string name, string email)
    {
        var user = new User{ Id = userId, Username = name, Email = email };
        await _usersRef.Document(userId).SetAsync(user);
    }

    public async Task RemoveUserAsync(string userId)
    {
        await _usersRef.Document(userId).DeleteAsync();
    }

    public async Task<List<User>> GetOnlineUsersAsync()
    {
        var snapshot = await _usersRef.GetSnapshotAsync();
        return snapshot.Documents
            .Select(doc => new User
            {
                Id = doc.Id,
                Username = doc.GetValue<string>("Username"),
                Email = doc.GetValue<string>("Email")
            })
            .ToList();
    }
}