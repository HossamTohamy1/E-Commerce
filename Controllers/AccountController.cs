using E_Commers.DTO;
using E_Commers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E_Commers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ECommerceDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ECommerceDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [Authorize(Roles = "Buyer")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "You must be logged in first" });
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.Email = request.Email ?? user.Email;

                if (!string.IsNullOrEmpty(request.NewPassword))
                {
                    var passwordChangeResult = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                    if (!passwordChangeResult.Succeeded)
                    {
                        return BadRequest(new { Message = "Error occurred while changing the password", Errors = passwordChangeResult.Errors });
                    }
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest(new { Message = "Error occurred while updating the account", Errors = updateResult.Errors });
                }

                await _signInManager.RefreshSignInAsync(user);

                return Ok(new { Message = "Account updated successfully" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "Error occurred while updating the account", Error = ex.Message });
            }
        }

        [Authorize(Roles = "Seller")]
        [HttpPost("add-store-profile")]
        public async Task<IActionResult> AddStoreProfile([FromBody] StoreProfileDTO storeProfileDto)
        {
            try
            {
                var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var existingStore = await _context.Stores
                    .FirstOrDefaultAsync(s => s.UserId == sellerId);

                if (existingStore != null)
                {
                    return BadRequest(new { message = "Store already exists for this seller." });
                }

                var newStore = new Store
                {
                    UserId = sellerId,
                    StoreName = storeProfileDto.StoreName,
                    Description = storeProfileDto.StoreDescription,
                };

                await _context.Stores.AddAsync(newStore);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Store profile added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding store profile", error = ex.Message });
            }
        }

        [Authorize(Roles = "Seller")]
        [HttpPut("update-store-profile")]
        public async Task<IActionResult> UpdateStoreProfile([FromBody] StoreProfileDTO storeProfileDto)
        {
            try
            {
                var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var store = await _context.Stores
                    .FirstOrDefaultAsync(s => s.UserId == sellerId);

                if (store == null)
                {
                    return NotFound(new { message = "Store not found." });
                }

                store.StoreName = storeProfileDto.StoreName ?? store.StoreName;
                store.Description = storeProfileDto.StoreDescription ?? store.Description;

                _context.Stores.Update(store);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Store profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating store profile", error = ex.Message });
            }
        }

        public class UpdateProfileRequest
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
