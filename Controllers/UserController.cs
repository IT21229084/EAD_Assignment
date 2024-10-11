using Microsoft.AspNetCore.Mvc;
using ECommerceAPI.Models;
using ECommerceAPI.Services;

namespace ECommerceAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/user
        [HttpGet]
        public async Task<List<User>> Get() =>
            await _userService.GetAsync();

        // GET: api/user/{id}
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return user;
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult<User>> Post(User newUser)
        {
            await _userService.CreateAsync(newUser);
            return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
        }

        // PUT: api/user/{id}
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(string id, User updatedUser)
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Preserve existing fields if they are not being updated
            updatedUser.Email = !string.IsNullOrEmpty(updatedUser.Email) ? updatedUser.Email : user.Email;
            updatedUser.FullName = !string.IsNullOrEmpty(updatedUser.FullName) ? updatedUser.FullName : user.FullName;
            updatedUser.Role = !string.IsNullOrEmpty(updatedUser.Role) ? updatedUser.Role : user.Role;
            updatedUser.PasswordHash = !string.IsNullOrEmpty(updatedUser.PasswordHash) ? updatedUser.PasswordHash : user.PasswordHash;

            // Keep the original Id
            updatedUser.Id = user.Id;

            await _userService.UpdateAsync(id, updatedUser);
            return Ok("User updated successfully.");
        }


        // DELETE: api/user/{id}
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            await _userService.RemoveAsync(id);
            return Ok("User deleted successfully.");
        }

        // POST: api/user/authenticate
        [HttpPost("authenticate")]
        public async Task<ActionResult<User>> Authenticate(string Email, string password)
        {
            var user = await _userService.AuthenticateAsync(Email, password);
            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }
            return Ok(user);
        }

        // PUT: api/user/{id}/role
        [HttpPut("{id:length(24)}/role")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] string role)
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            await _userService.UpdateRoleAsync(id, role);
            return Ok($"User role updated to {role}.");
        }

        // PUT: api/user/{id}/status
        [HttpPut("{id:length(24)}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] bool isActive)
        {
            var user = await _userService.GetAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            await _userService.UpdateStatusAsync(id, isActive);
            return Ok($"User status updated to {(isActive ? "Active" : "Inactive")}.");
        }
    }
}
