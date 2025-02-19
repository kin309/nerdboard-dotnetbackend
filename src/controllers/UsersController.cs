using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        await _userService.AddUserAsync(user.Id, user.Username, user.Email);
        return Ok();
    }

    [HttpDelete("remove/{userId}")]
    public async Task<IActionResult> RemoveUser(string userId)
    {
        await _userService.RemoveUserAsync(userId);
        return Ok();
    }

    [HttpGet("online")]
    public async Task<IActionResult> GetOnlineUsers()
    {
        var users = await _userService.GetOnlineUsersAsync();
        return Ok(users);
    }
}