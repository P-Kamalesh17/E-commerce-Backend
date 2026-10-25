using DAL.DTO;

namespace BLL
{
    public interface IUserService
    {
        Task CreateUserAsync(CreateUserVM user);
        Task<List<UserVM>> GetAllUsersAsync();
        Task<UserVM> GetUserByIdAsync(string id);
        Task UpdateUserByIdAsync(string id, UserVM user);
        Task DeleteUserByIdAsync(string id);
        Task<List<OrderVM>> GetUserOrderById(string id);
    }
}
