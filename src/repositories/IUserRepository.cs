public interface IUserRepository
{
    Task AddUserAsync(FirestoreUser user);
    Task RemoveUserAsync(string userId);
    Task<List<FirestoreUser>> GetOnlineUsersAsync();
}