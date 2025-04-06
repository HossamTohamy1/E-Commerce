using E_Commers.DTO;
using E_Commers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static E_Commers.DTO.ProductDto;

namespace E_Commers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ECommerceDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ECommerceDbContext context, ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        [HttpPost("PromoteUser")]
        public async Task<IActionResult> PromoteUser(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Error = "User not found" });
            }

            var roleExists = await _roleManager.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                return BadRequest(new { Error = "Role does not exist" });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(newRole))
            {
                return BadRequest(new { Error = "User is already in this role" });
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (result.Succeeded)
            {
                return Ok(new { Message = $"User promoted to {newRole}" });
            }

            return BadRequest(new { Error = "Failed to promote user", Details = result.Errors });
        }

        [HttpPost("DemoteUser")]
        public async Task<IActionResult> DemoteUser(string userId, string roleToRemove)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Error = "User not found" });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(roleToRemove))
            {
                return BadRequest(new { Error = $"User is not assigned the {roleToRemove} role" });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleToRemove);
            if (result.Succeeded)
            {
                return Ok(new { Message = $"User demoted by removing the {roleToRemove} role" });
            }

            return BadRequest(new { Error = "Failed to demote user", Details = result.Errors });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error occurred while retrieving users", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-user/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UserDTO userDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.Email = userDto.Email;
                user.Role = userDto.Role;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error occurred while updating the user", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error occurred while deleting the user", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Admin-update-product/{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductForAdminDTO productDto)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    return NotFound(new { message = "Product not found." });
                }

                product.Title = productDto.Name ?? product.Title;
                product.Description = productDto.Description ?? product.Description;
                product.Price = productDto.Price ?? product.Price;
                product.StockQuantity = productDto.StockQuantity ?? product.StockQuantity;
                product.MainImageUrl = productDto.ProductImage ?? product.MainImageUrl;
                product.SKU = productDto.SKU ?? product.SKU;

                var productCategory = await _context.ProductCategories.FirstOrDefaultAsync(pc => pc.ProductId == productId);

                if (productCategory != null)
                {
                    productCategory.CategoryId = productDto.CategoryId ?? productCategory.CategoryId;
                    _context.ProductCategories.Update(productCategory);
                }
                else
                {
                    return NotFound(new { message = "Product category not found." });
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating product", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("Admin-delete-product/{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    return NotFound(new { message = "Product not found." });
                }

                var productSellers = await _context.ProductSellers.Where(ps => ps.ProductId == productId).ToListAsync();
                _context.ProductSellers.RemoveRange(productSellers);

                var productCategories = await _context.ProductCategories.Where(pc => pc.ProductId == productId).ToListAsync();
                _context.ProductCategories.RemoveRange(productCategories);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting product", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-category")]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDTO categoryDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryDto.Name);

                    if (existingCategory != null)
                    {
                        return BadRequest(new { message = "Category with the same name already exists." });
                    }

                    var category = new Category
                    {
                        Name = categoryDto.Name,
                        Description = categoryDto.Description
                    };

                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();

                    if (categoryDto.ProductIds != null && categoryDto.ProductIds.Any())
                    {
                        foreach (var productId in categoryDto.ProductIds)
                        {
                            var productCategory = new ProductCategory
                            {
                                ProductId = productId,
                                CategoryId = category.CategoryId
                            };

                            _context.ProductCategories.Add(productCategory);
                        }

                        await _context.SaveChangesAsync();
                    }

                    return Ok(new { message = "Category added successfully" });
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving category: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while adding the category", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user/{id}/export-data")]
        public async Task<IActionResult> ExportUserData(string id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                var orders = await _context.Orders.Where(o => o.UserId == id).Select(o => new OrderExportDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount
                }).ToListAsync();

                var complaints = await _context.Complaints.Where(c => c.UserId == id).Select(c => new ComplaintDTO
                {
                    ComplaintId = c.ComplaintId,
                    ComplaintDetails = c.Description,
                    DateFiled = c.ComplaintDate
                }).ToListAsync();

                var export = new UserExportDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Orders = orders,
                    Complaints = complaints
                };

                return Ok(export);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting user data", error = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("block-user/{userId}")]
        public async Task<IActionResult> BlockUser(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Block the user
                user.IsBlocked = true;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "User has been blocked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while blocking the user", Error = ex.Message });
            }
        }

        // Unblock user
        [Authorize(Roles = "Admin")]
        [HttpPost("unblock-user/{userId}")]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Unblock the user
                user.IsBlocked = false;
                await _context.SaveChangesAsync();

                return Ok(new { Message = "User has been unblocked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while unblocking the user", Error = ex.Message });
            }
        }
    }
}