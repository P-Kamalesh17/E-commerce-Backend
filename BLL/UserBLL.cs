using DAL.DTO;
using DAL.Services;

namespace BLL
{
    public class UserBLL : IUserService
    {
        private readonly UserDAO _userDAO;

        public UserBLL(UserDAO userDAO)
        {
            _userDAO = userDAO;
        }

        public async Task CreateUserAsync(CreateUserVM user)
        {
            await _userDAO.CreateUserAsync(user);
        }

        public async Task<List<UserVM>> GetAllUsersAsync()
        {
            return await _userDAO.GetAllUsersAsync();
        }

        public async Task<UserVM> GetUserByIdAsync(string id)
        {
            return await _userDAO.GetUserByIdAsync(id);
        }

        public async Task UpdateUserByIdAsync(
            string id,
            UserVM user)
        {
            await _userDAO.UpdateUserByIdAsync(id, user);
        }

        public async Task DeleteUserByIdAsync(string id)
        {
            await _userDAO.DeleteUserByIdAsync(id);
        }

        public async Task<List<OrderVM>> GetUserOrderById(string id)
        {
            return await _userDAO.GetUserOrderById(id);
        }
    }
}