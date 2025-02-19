using Microsoft.AspNetCore.SignalR;

public class UserHub : Hub
{
    private readonly UserService _userService;

    public UserHub(UserService userService)
    {
        _userService = userService;
    }

    public async Task AddUser(string userId, string username, string email)
    {
        await _userService.AddUserAsync(userId, username, email);
        await Clients.All.SendAsync("UserAdded", userId, username);
    }

    public async Task RemoveUser(string userId)
    {
        await _userService.RemoveUserAsync(userId);
        await Clients.All.SendAsync("UserRemoved", userId);
    }

    public async Task<List<User>> GetOnlineUsers()
    {
        return await _userService.GetOnlineUsersAsync();
    }
}