using DAL.Data;
using DAL.DTO;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Services
{
    public class UserDAO
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserDAO(AppDbContext context,UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //private async Task<User> CheckValidIdExistAsync(string id)
        //{
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        throw new ArgumentException("User ID is required.");
        //    }

        //    var user = await _userManager.FindByIdAsync(id);

        //    if (user == null)
        //    {
        //        throw new KeyNotFoundException("User not found.");
        //    }

        //    return user;
        //}

        // CREATE USER
        public async Task CreateUserAsync(CreateUserVM model)
        {
            if (model == null)
            {
                throw new ArgumentException("User data is required.");
            }
            
            var existingUser = await _userManager
                .FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "A user with this email already exists.");
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber
            };

            // Identity generates the ID and hashes the password.
            var result = await _userManager.CreateAsync(
                user,
                model.Password);


            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(errors);
            }
        }

        // GET ALL USERS
        public async Task<List<UserVM>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserVM
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Address = u.Address,
                    PhoneNumber = u.PhoneNumber,
                })
                .ToListAsync();
        }

        // GET USER BY ID
        public async Task<UserVM> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            return new UserVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };
        }

        // UPDATE USER
        public async Task UpdateUserByIdAsync(
            string id,
            UserVM model)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID is required.");
            }

            if (model == null)
            {
                throw new ArgumentException("User data is required.");
            }

            var existingUser = await _userManager.FindByIdAsync(id);

            if (existingUser == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var emailUser = await _userManager
                .FindByEmailAsync(model.Email);

            if (emailUser != null && emailUser.Id != id)
            {
                throw new InvalidOperationException(
                    "This email is already used by another user.");
            }

            existingUser.FullName = model.FullName;
            existingUser.Address = model.Address;
            existingUser.PhoneNumber = model.PhoneNumber;

            if (existingUser.Email != model.Email)
            {
                var emailResult = await _userManager
                    .SetEmailAsync(existingUser, model.Email);

                if (!emailResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        emailResult.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(errors);
                }

                var usernameResult = await _userManager
                    .SetUserNameAsync(existingUser, model.Email);

                if (!usernameResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        usernameResult.Errors.Select(
                            error => error.Description));

                    throw new InvalidOperationException(errors);
                }
            }

            var result = await _userManager.UpdateAsync(existingUser);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(errors);
            }
        }

        // DELETE USER
        public async Task DeleteUserByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID is required.");
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(errors);
            }
        }

        public async Task<List<OrderVM>> GetUserOrderById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("User ID is required.");
            }

            var _order = await _context.Orders
                .Where(order => order.UserId == id)
                .Select(order => new OrderVM
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    OrderStatus = order.Status.ToString(),
                    ShippingAddress = order.ShippingAddress,
                    TotalAmount = order.TotalAmount,
                    CreatedAt = order.CreatedAt,
                    PaymentStatus = _context.Payments
                        .Where(p => p.OrderId == order.Id)
                        .Select(p => p.Status.ToString())
                        .FirstOrDefault() ?? PaymentStatus.Pending.ToString()
                }).ToListAsync();

            return _order;


        }
    }
}