using DAL.DTO;

namespace BLL
{
    public interface ICartService
    {
        Task<CartVM?> GetCartByUserId(string userId);
        Task<CartSummaryVM> GetCartSummary(string userId);
        Task AddItem(AddCartItemVM addCartItemVM);
        Task UpdateItemQuantity(int cartItemId, UpdateCartItemVM updateCartItemVM);
        Task RemoveItem(int cartItemId);
        Task ClearCart(string userId);
        Task<OrderVM> Checkout(CheckoutVM checkoutVM);
    }
}
