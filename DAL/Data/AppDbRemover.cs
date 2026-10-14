using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class AppDbRemover
    {
        private readonly AppDbContext _context;

        public AppDbRemover(AppDbContext context)
        {
            _context = context;
        }

        public async Task TruncateAllDataAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT all'");

                await _context.RefreshTokens.ExecuteDeleteAsync();
                await _context.Order_Items.ExecuteDeleteAsync();
                await _context.Orders.ExecuteDeleteAsync();
                await _context.CartItems.ExecuteDeleteAsync();
                await _context.Carts.ExecuteDeleteAsync();
                await _context.Products.ExecuteDeleteAsync();
                await _context.Categories.ExecuteDeleteAsync();
                await _context.Payments.ExecuteDeleteAsync();
                await _context.Users.ExecuteDeleteAsync();

                await _context.Database.ExecuteSqlRawAsync("EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'");

                // Reset identity seeds
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('RefreshTokens', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Order_Items', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Orders', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('CartItems', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Carts', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Products', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Categories', RESEED, 0)");
                await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Payments', RESEED, 0)");

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error truncating database tables.", ex);
            }
        }
    }
}
