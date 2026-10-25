using DAL.DTO;
using DAL.Services;

namespace BLL
{
    public class CartBLL : ICartService
    {
        private readonly CartDAO _cartDAO;

        public CartBLL(CartDAO cartDAO)
        {
            _cartDAO = cartDAO;
        }

        public async Task<CartVM?> GetCartByUserId(string userId)
        {
            return await _cartDAO.GetCartByUserId(userId);
        }

        public async Task<CartSummaryVM> GetCartSummary(string userId)
        {
            return await _cartDAO.GetCartSummary(userId);
        }

        public async Task AddItem(AddCartItemVM addCartItemVM)
        {
            await _cartDAO.AddItem(addCartItemVM);
        }

        public async Task UpdateItemQuantity(int cartItemId, UpdateCartItemVM updateCartItemVM)
        {
            await _cartDAO.UpdateItemQuantity(cartItemId, updateCartItemVM);
        }

        public async Task RemoveItem(int cartItemId)
        {
            await _cartDAO.RemoveItem(cartItemId);
        }

        public async Task ClearCart(string userId)
        {
            await _cartDAO.ClearCart(userId);
        }

        public async Task<OrderVM> Checkout(CheckoutVM checkoutVM)
        {
            return await _cartDAO.Checkout(checkoutVM);
        }
    }
}
