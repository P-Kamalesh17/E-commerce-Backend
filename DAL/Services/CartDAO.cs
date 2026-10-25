using DAL.Data;
using DAL.DTO;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Services
{
    public class CartDAO
    {
        private readonly AppDbContext _context;

        public CartDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartVM?> GetCartByUserId(string userId)
        {
            var cart = await GetCartWithItems(userId);

            if (cart == null)
            {
                return null;
            }

            return MapToCartVM(cart);
        }

        public async Task<CartSummaryVM> GetCartSummary(string userId)
        {
            var cart = await GetCartWithItems(userId);

            if (cart == null)
            {
                throw new KeyNotFoundException("Cart not found for this user.");
            }

            return ComputeSummary(cart);
        }

        public async Task AddItem(AddCartItemVM addCartItemVM)
        {
            if (addCartItemVM == null)
            {
                throw new ArgumentNullException(
                    nameof(addCartItemVM),
                    "Cart item details are required."
                );
            }

            if (string.IsNullOrWhiteSpace(addCartItemVM.UserId))
            {
                throw new ArgumentException("User ID is required.");
            }

            if (addCartItemVM.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == addCartItemVM.ProductId);

            if (product == null)
            {
                throw new KeyNotFoundException("Invalid product ID.");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == addCartItemVM.UserId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = addCartItemVM.UserId
                };

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == addCartItemVM.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += addCartItemVM.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;   
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = addCartItemVM.ProductId,
                    Quantity = addCartItemVM.Quantity
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemQuantity(int cartItemId, UpdateCartItemVM updateCartItemVM)
        {
            if (updateCartItemVM == null)
            {
                throw new ArgumentNullException(
                    nameof(updateCartItemVM),
                    "Cart item details are required."
                );
            }

            if (updateCartItemVM.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero.");
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
            {
                throw new KeyNotFoundException("Invalid cart item ID.");
            }

            cartItem.Quantity = updateCartItemVM.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveItem(int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
            {
                throw new KeyNotFoundException("Invalid cart item ID.");
            }

            _context.CartItems.Remove(cartItem);
            cartItem.Cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task ClearCart(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                throw new KeyNotFoundException("Cart not found for this user.");
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<OrderVM> Checkout(CheckoutVM checkoutVM)
        {
            if (checkoutVM == null)
            {
                throw new ArgumentNullException(
                    nameof(checkoutVM),
                    "Checkout details are required."
                );
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == checkoutVM.UserId);

            checkoutVM.ShippingAddress = existingUser?.Address;

            if (string.IsNullOrWhiteSpace(checkoutVM.ShippingAddress))
            {
                throw new ArgumentException(
                    "A shipping address is required, and no saved address was found for this user.",
                    nameof(checkoutVM.ShippingAddress)
                );
            }

            var cart = await GetCartWithItems(checkoutVM.UserId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cart is empty.");
            }

            foreach (var item in cart.CartItems)
            {
                if (item.Product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.Product.Name}'."
                    );
                }
            }

            var summary = ComputeSummary(cart);

            var order = new Order
            {
                UserId = checkoutVM.UserId,
                ShippingAddress = checkoutVM.ShippingAddress,
                Status = OrderStatus.Pending,
                TotalAmount = summary.TotalAmount
            };

            foreach (var item in cart.CartItems)
            {
                order.OrderItems.Add(new Order_Item
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    Subtotal = item.Product.Price * item.Quantity
                });

                item.Product.StockQuantity -= item.Quantity;
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OrderVM
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderStatus = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            };
        }

        private async Task<Cart?> GetCartWithItems(string userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private CartSummaryVM ComputeSummary(Cart cart)
        {
            var subtotal = cart.CartItems
                .Sum(ci => ci.Product.Price * ci.Quantity);

            var discountAmount = 0m;
            var tax = 0m;
            var shippingCost = 0m;

            return new CartSummaryVM
            {
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                Tax = tax,
                ShippingCost = shippingCost,
                TotalAmount = subtotal - discountAmount + tax + shippingCost
            };
        }

        private CartVM MapToCartVM(Cart cart)
        {
            return new CartVM
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                Items = cart.CartItems.Select(ci => new CartItemVM
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.Product.Price,
                    Quantity = ci.Quantity,
                    Subtotal = ci.Product.Price * ci.Quantity
                }).ToList()
            };
        }
    }
}
