using E_Commers.DTO;
using E_Commers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static E_Commers.DTO.ProductDto;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ECommerceDbContext _context;

    public ProductsController(ECommerceDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new Product
                {
                    ProductId = p.ProductId,
                    Title = p.Title,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                }).ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving products.", error = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        try
        {
            var product = await _context.Products
                .Where(p => p.ProductId == id && p.IsActive)
                .Select(p => new Product
                {
                    ProductId = p.ProductId,
                    Description = p.Description,
                    Title = p.Title,
                    Price = p.Price,
                    IsActive = p.IsActive,
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Product not found." });

            return Ok(product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the product.", error = ex.Message });
        }
    }

    [Authorize(Roles = "Seller")]
    [HttpPost("add-product")]
    public async Task<IActionResult> AddProduct([FromBody] ProductDTO productDto)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var seller = await _context.Users.FirstOrDefaultAsync(s => s.Id == sellerId);
                if (seller == null)
                {
                    return BadRequest(new { message = "Seller is not registered in the system" });
                }

                var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.CategoryId == productDto.CategoryId);
                if (category == null)
                {
                    return BadRequest(new { message = "Category not found" });
                }

                var product = new Product
                {
                    Title = productDto.ProductName,
                    Description = productDto.ProductDescription,
                    Price = productDto.Price,
                    StockQuantity = productDto.StockQuantity,
                    MainImageUrl = productDto.ProductImage,
                    SKU = productDto.SKU,
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var productId = product.ProductId;

                var productSeller = new ProductSeller
                {
                    ProductId = productId,
                    UserId = sellerId,
                    SellerPrice = productDto.SellerPrice,
                    StockInSeller = productDto.SellerStockQuantity,
                };

                _context.ProductSellers.Add(productSeller);

                var productCategory = new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = productDto.CategoryId
                };
                _context.ProductCategories.Add(productCategory);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Product added successfully" });
            }

            return BadRequest(ModelState);
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, new { message = "Error occurred while adding the product", error = dbEx.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
        }
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("update-product/{productId}")]
    public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductDTO productDto)
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var productSeller = await _context.ProductSellers
                .Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.UserId == sellerId);

            if (productSeller == null)
            {
                return NotFound(new { message = "Product not found or you are not authorized to edit it" });
            }

            var product = productSeller.Product;
            product.Title = productDto.ProductName;
            product.Description = productDto.ProductDescription;
            product.Price = productDto.Price;
            product.StockQuantity = productDto.StockQuantity;
            product.MainImageUrl = productDto.ProductImage;
            product.SKU = productDto.SKU;

            var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.CategoryId == productDto.CategoryId);
            if (category == null)
            {
                return BadRequest(new { message = "Category not found" });
            }

            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.ProductId == productId);

            if (productCategory != null)
            {
                productCategory.CategoryId = productDto.CategoryId;
            }
            else
            {
                var newProductCategory = new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = productDto.CategoryId
                };
                _context.ProductCategories.Add(newProductCategory);
            }

            productSeller.SellerPrice = productDto.SellerPrice;
            productSeller.StockInSeller = productDto.SellerStockQuantity;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the product", error = ex.Message });
        }
    }

    [Authorize(Roles = "Seller")]
    [HttpDelete("delete-product/{productId}")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var productSeller = await _context.ProductSellers
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.UserId == sellerId);

            if (productSeller == null)
            {
                return NotFound(new { message = "Product not found or you are not authorized to delete it" });
            }

            _context.ProductSellers.Remove(productSeller);

            var otherSellers = await _context.ProductSellers
                .AnyAsync(ps => ps.ProductId == productId && ps.UserId != sellerId);

            if (!otherSellers)
            {
                var product = await _context.Products.FindAsync(productId);
                if (product != null)
                {
                    _context.Products.Remove(product);

                    var productCategories = await _context.ProductCategories
                        .Where(pc => pc.ProductId == productId)
                        .ToListAsync();

                    if (productCategories.Any())
                    {
                        _context.ProductCategories.RemoveRange(productCategories);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the product", error = ex.Message });
        }
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("update-stock-price/{productId}")]
    public async Task<IActionResult> UpdateSellerStockAndPrice(int productId, [FromBody] UpdateStockPriceDTO dto)
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var productSeller = await _context.ProductSellers
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.UserId == sellerId);

            if (productSeller == null)
            {
                return NotFound(new { message = "You are not authorized to manage this product's stock or it doesn't exist" });
            }

            productSeller.StockInSeller = dto.SellerStockQuantity;
            productSeller.SellerPrice = dto.SellerPrice;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Stock and price updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating stock and price", error = ex.Message });
        }
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetSellerOrders()
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.OrderItems
                .Where(oi => _context.ProductSellers
                            .Any(ps => ps.ProductId == oi.ProductId && ps.UserId == sellerId))
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Select(oi => new
                {
                    OrderId = oi.OrderId,
                    ProductName = oi.Product.Title,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.UnitPrice,
                    OrderStatus = oi.Order.OrderStatus,
                    OrderDate = oi.Order.OrderDate
                })
                .ToListAsync();

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error while fetching your orders", error = ex.Message });
        }
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("update-order-status")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDTO dto)
    {
        try
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sellerOwnsOrder = await _context.OrderItems
                .AnyAsync(oi => oi.OrderId == dto.OrderId &&
                                _context.ProductSellers
                                    .Any(ps => ps.ProductId == oi.ProductId && ps.UserId == sellerId));

            if (!sellerOwnsOrder)
            {
                return Forbid("You are not authorized to update this order");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == dto.OrderId);
            if (order == null)
                return NotFound("Order not found");

            order.OrderStatus = dto.NewStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}
