using Google.Cloud.Firestore;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task AddUserAsync(string userId, string? name, string? email)
    {
        var user = new FirestoreUser{ Id = userId, Username = name, Email = email };
        await _userRepository.AddUserAsync(user);
    }

    public async Task RemoveUserAsync(string userId)
    {
        await _userRepository.RemoveUserAsync(userId);
    }

    public async Task<List<FirestoreUser>> GetOnlineUsersAsync()
    {
        return await _userRepository.GetOnlineUsersAsync();
    }
}