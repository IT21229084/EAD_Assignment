using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Services;
using System.Threading.Tasks;
using ECommerceAPI.Models;
namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        public AuthController(UserService userService, AuthService AuthService)
        {
            _authService = AuthService;
            _userService = userService;

        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginRequest request)
        //{
        //    var token = await _authService.AuthenticateAsync(request.Email, request.Password);

        //    if (string.IsNullOrEmpty(token))
        //        return Unauthorized("Invalid email or password.");

        //    return Ok(new { Token = token });
        //}

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginModel model)
        //{
        //    var user = await _userService.GetByeEmailAsync(model.Email);
        //    if (user == null || !UserService.VerifyPassword(model.Password, user.PasswordHash) || user.IsActive == true)
        //    {
        //        return Unauthorized("Invalid credentials or inactive account.");
        //    }

        //    var token = _authService.GenerateJwtToken(user);
        //    return Ok(new { Token = token, Role = user.Role });
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userService.GetByeEmailAsync(model.Email);

            // Check if the user exists
            if (user == null)
            {
                return Unauthorized("Invalid email.");
            }

            // Check if the password is correct
            if (!UserService.VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid password.");
            }

            // Check if the account is active
            if (!user.IsActive)
            {
                return Unauthorized("Account is inactive.");
            }

            // Generate the JWT token if everything is correct
            var token = _authService.GenerateJwtToken(user);
            return Ok(new { Token = token, Role = user.Role });
        }

    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
