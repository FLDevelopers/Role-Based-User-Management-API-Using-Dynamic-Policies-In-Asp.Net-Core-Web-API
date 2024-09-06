using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiWithRoleAuthentication.Models;

namespace WebApiWithRoleAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            if (!isValidEmail(model.Email)) 
            {
                return BadRequest(new {message = "Invalid email format." });
            }
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if(existingUser != null)
            {
                return BadRequest(new { message = "Email already exists." });
            }


            var user = new IdentityUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.phoneNumber  };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if(!await _roleManager.RoleExistsAsync("User"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("User"));
                    if (!roleResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(user);
                        return StatusCode(500, new { message = "User role creation failed.", errors = roleResult.Errors });
                    }
                }

                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new { message = "User registered successfully" });
            }

            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new {message = "Registration Failed.",errors});
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim("userId", user.Id)
                };

                authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                    SecurityAlgorithms.HmacSha256));

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
        }

        private bool isValidEmail(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
