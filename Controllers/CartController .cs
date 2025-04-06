using E_Commers.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

[Authorize(Roles = "Buyer,Seller")]
[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ECommerceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ECommerceDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
                return Ok(new { message = "Your cart is empty" });

            var cartDTO = new CartDTO
            {
                CartId = cart.CartId,
                CreatedAt = cart.CreatedAt,
                Items = cart.Items.Select(item => new CartItemDTO
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    AddedAt = item.AddedAt
                }).ToList()
            };

            return Ok(cartDTO);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCart: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while retrieving the cart.", error = ex.Message });
        }
    }

    [Authorize(Roles = "Buyer")]
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null || !product.IsActive)
                return NotFound("Product not found");

            if (product.StockQuantity < dto.Quantity)
                return BadRequest("Not enough stock available");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            else if (cart.Items == null)
            {
                cart.Items = new List<CartItem>();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    AddedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added to cart successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddToCart: {ex.Message}");
            return StatusCode(500, new
            {
                message = "An error occurred while adding the product to the cart.",
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [Authorize(Roles = "Buyer")]
    [HttpDelete("remove/{productId}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove == null)
                return NotFound("Product not found in cart");

            cart.Items.Remove(itemToRemove);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product removed from cart successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RemoveFromCart: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while removing the product from the cart.", error = ex.Message });
        }
    }

    [Authorize(Roles = "Buyer")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null)
                return NotFound("Product not found in cart");

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product.StockQuantity < dto.Quantity)
                return BadRequest("Not enough stock available");

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "حدث خطأ أثناء معالجة الطلب",
                Error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [Authorize(Roles = "Buyer")]
    [HttpPost("complete-purchase")]
    public async Task<IActionResult> CompletePurchase([FromBody] PurchaseRequest request)
    {
        try
        {
            var buyerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(buyerId))
                return Unauthorized(new { Message = "You must be logged in first." });

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == buyerId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return BadRequest(new { Message = "Shopping cart is empty." });

            var shippingAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == request.ShippingAddressId);

            if (shippingAddress == null)
            {
                shippingAddress = new Address
                {
                    AddressLine1 = request.AddressLine1,
                    City = request.City,
                    Country = request.Country,
                    PostalCode = request.PostalCode,
                    State = request.State ?? "N/A",
                    AddressType = "Shipping",
                    UserId = buyerId
                };
                _context.Addresses.Add(shippingAddress);
                await _context.SaveChangesAsync();
            }

            var order = new Order
            {
                UserId = buyerId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Processing",
                TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                TransactionId = request.TransactionId,
                ShippingAddressId = shippingAddress.AddressId,
                PaymentMethod = request.PaymentMethod
            };

            if (order.OrderItems == null)
            {
                order.OrderItems = new List<OrderItem>();
            }

            foreach (var item in cart.Items)
            {
                if (item.Product == null)
                    return BadRequest(new { Message = "Product not found in cart." });

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });

                item.Product.StockQuantity -= item.Quantity;
                if (item.Product.StockQuantity < 0)
                    return BadRequest(new { Message = $"Not enough stock for product: {item.Product.Title}" });
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Purchase completed successfully.",
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount,
                Products = order.OrderItems.Select(i => new
                {
                    i.ProductId,
                    i.Product.Title,
                    i.Quantity,
                    i.UnitPrice
                })
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "An error occurred while processing the order.",
                Error = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
}

public class CartItemDTO
{
    public int CartItemId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CartDTO
{
    public int CartId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CartItemDTO> Items { get; set; }
}

public class AddToCartDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class PurchaseRequest
{
    public int? ShippingAddressId { get; set; }
    public string AddressLine1 { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string PaymentMethod { get; set; }
    public string State { get; set; }
    public string TransactionId { get; set; }
}

public class ReviewRequest
{
    public int ProductId { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; }
}
