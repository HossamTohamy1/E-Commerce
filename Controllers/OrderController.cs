using E_Commers.DTO;
using E_Commers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ECommerceDbContext _context;
        public OrderController(ECommerceDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Buyer")]
        [HttpGet("track-order/{orderId}")]
        public async Task<IActionResult> TrackOrder(int orderId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "You must be logged in first." });
                }

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.ShippingAddress)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

                if (order == null)
                {
                    return NotFound(new { Message = "Order not found." });
                }

                if (order.ShippingAddress == null)
                {
                    return BadRequest(new { Message = "Shipping address not found." });
                }

                return Ok(new
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = new
                    {
                        order.ShippingAddress.AddressLine1,
                        order.ShippingAddress.City,
                        order.ShippingAddress.State,
                        order.ShippingAddress.Country,
                        order.ShippingAddress.PostalCode
                    },
                    OrderItems = order.OrderItems.Select(oi => new
                    {
                        oi.Product.Title,
                        oi.Quantity,
                        oi.UnitPrice
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while tracking the order.", Error = ex.Message });
            }
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost("request-seller-upgrade")]
        public async Task<IActionResult> RequestSellerUpgrade([FromBody] SellerUpgradeRequestModel request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "You must be logged in first" });
                }

                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                if (user.Role.Contains("Seller"))
                {
                    return BadRequest(new { Message = "You are already a seller" });
                }

                if (string.IsNullOrEmpty(request.BusinessName) || string.IsNullOrEmpty(request.ContactInfo))
                {
                    return BadRequest(new { Message = "Business name and contact information are required" });
                }

                var sellerUpgradeRequest = new SellerUpgradeRequest
                {
                    UserId = userId,
                    BusinessName = request.BusinessName,
                    ContactInfo = request.ContactInfo,
                    RequestDate = DateTime.UtcNow,
                    Status = "Pending"
                };

                _context.SellerUpgradeRequests.Add(sellerUpgradeRequest);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Your request to become a seller has been submitted successfully" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Message = "Error occurred while processing the upgrade request", Error = ex.Message });
            }
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost("add-address")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDTO addressDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (addressDto.IsDefault)
                    {
                        var existingAddresses = await _context.Addresses
                            .Where(a => a.UserId == userId && a.IsDefault)
                            .ToListAsync();

                        foreach (var addr in existingAddresses)
                        {
                            addr.IsDefault = false;
                        }
                    }

                    var newAddress = new Address
                    {
                        UserId = userId,
                        AddressLine1 = addressDto.AddressLine1,
                        AddressLine2 = addressDto.AddressLine2,
                        City = addressDto.City,
                        State = addressDto.State,
                        PostalCode = addressDto.PostalCode,
                        Country = addressDto.Country,
                        IsDefault = addressDto.IsDefault,
                        AddressType = addressDto.AddressType
                    };

                    _context.Addresses.Add(newAddress);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Address added successfully", addressId = newAddress.AddressId });
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the address", error = ex.Message });
            }
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost("add-order")]
        public async Task<IActionResult> AddOrder([FromBody] OrderDTO orderDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var cartItems = await _context.CartItems
                        .Where(ci => ci.Cart.UserId == userId)
                        .Join(_context.Products, ci => ci.ProductId, p => p.ProductId, (ci, p) => new
                        {
                            CartItem = ci,
                            Product = p
                        })
                        .ToListAsync();

                    if (cartItems == null || !cartItems.Any())
                    {
                        return BadRequest(new { message = "The cart is empty or does not exist" });
                    }

                    decimal totalAmount = cartItems.Sum(ci => ci.Product.Price * ci.CartItem.Quantity);

                    var order = new Order
                    {
                        UserId = userId,
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = totalAmount,
                        OrderStatus = "Pending",
                        ShippingAddressId = orderDto.ShippingAddressId,
                        PaymentMethod = orderDto.PaymentMethod,
                        TransactionId = orderDto.TransactionId
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    foreach (var cartItem in cartItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            ProductId = cartItem.Product.ProductId,
                            Quantity = cartItem.CartItem.Quantity,
                            UnitPrice = cartItem.Product.Price
                        };
                        _context.OrderItems.Add(orderItem);
                    }
                    await _context.SaveChangesAsync();

                    _context.CartItems.RemoveRange(cartItems.Select(ci => ci.CartItem));
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Order has been added successfully" });
                }

                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while adding the order",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [Authorize(Roles = "Seller")]
        [HttpGet("sales-report")]
        public async Task<IActionResult> GetSalesReport()
        {
            try
            {
                var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var orderItems = await _context.OrderItems
                     .Include(oi => oi.Product)
                     .ThenInclude(p => p.ProductSellers)
                     .Include(oi => oi.Order)
                     .Where(oi => oi.Product.ProductSellers.Any(ps => ps.UserId == sellerId))
                    .ToListAsync();

                if (!orderItems.Any())
                {
                    return Ok(new { message = "No sales data available." });
                }

                var totalOrders = orderItems.Select(oi => oi.OrderId).Distinct().Count();
                var totalProductsSold = orderItems.Sum(oi => oi.Quantity);
                var totalRevenue = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
                var uniqueBuyers = orderItems.Select(oi => oi.Order.UserId).Distinct().Count();

                var topProducts = orderItems
                    .GroupBy(oi => new { oi.ProductId, oi.Product.Title })
                    .Select(g => new TopProductDTO
                    {
                        ProductName = g.Key.Title,
                        QuantitySold = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(tp => tp.QuantitySold)
                    .Take(5)
                    .ToList();

                var report = new SalesReportDTO
                {
                    TotalOrders = totalOrders,
                    TotalProductsSold = totalProductsSold,
                    TotalRevenue = totalRevenue,
                    UniqueBuyers = uniqueBuyers,
                    TopProducts = topProducts
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating sales report", error = ex.Message });
            }
        }

        public class SellerUpgradeRequestModel
        {
            public string UserId { get; set; }
            public string BusinessName { get; set; }
            public string ContactInfo { get; set; }
            public DateTime RequestDate { get; set; }
            public string Status { get; set; }
        }
    }
}
