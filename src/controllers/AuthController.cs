using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromHeader] string Authorization)
    {
        var token = Authorization?.Replace("Bearer ", "");

        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
            return Ok(new { message = "Autenticado com sucesso", userId = decodedToken.Uid });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Unauthorized();
        }
    }
}