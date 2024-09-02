using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApiWithRoleAuthentication.Models;

namespace WebApiWithRoleAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "RoleUser")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;

        public UserController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo([FromBody] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) 
            {
                return NotFound("User not found.");
            }

            return Ok(new {user.Id, user.Email, user.PhoneNumber,});
        }

        [HttpPut("user-info")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserInfo model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded) 
            {
                return Ok(new { message="User Info Updated Successfully." });
            }

            return BadRequest(result.Errors);
        }

        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUserAccount([FromBody] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded) 
            {
                return Ok(new {message="User deleted successfully."});
            }

            return BadRequest(result.Errors);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePassword model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded) 
            {
                return Ok(new { message = "Password changed successfully." });
            }

            return BadRequest(result.Errors);
        }
    }
}
