using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using E_Commers.Models;
using System.ComponentModel.DataAnnotations;

namespace ECommerceAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("InitializeRoles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InitializeRoles()
        {
            try
            {
                string[] roles = { "Admin", "Seller", "Buyer" };

                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                return Ok(new { Message = "Roles initialized successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    Role = "Buyer"
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Buyer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Buyer"));
                    }

                    await _userManager.AddToRoleAsync(user, "Buyer");

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return Ok(new
                    {
                        Message = "User registered successfully as Buyer.",
                        UserId = user.Id
                    });
                }

                return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An unexpected error occurred.",
                    Details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    return Unauthorized(new { Error = "Invalid email or password." });
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized(new { Error = "Invalid email or password." });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName)
                };
                
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddHours(13),
                    claims: claims,
                    signingCredentials: signingCredentials
                );

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    UserInfo = new
                    {
                        user.Id,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        Roles = userRoles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An unexpected error occurred.",
                    Details = ex.Message
                });
            }
        }
    }

    public class RegisterModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RoleAssignmentModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
