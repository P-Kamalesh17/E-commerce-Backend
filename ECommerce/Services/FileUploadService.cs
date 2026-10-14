using System;
using System.IO;
using System.Threading.Tasks;

namespace ECommerce.API.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file);
        bool DeleteImage(string imagePath);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadsFolderPath;
        private readonly string _baseUrl;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(ILogger<FileUploadService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _baseUrl = configuration["FileUpload:BaseUrl"] ?? "https://localhost:7080";
            _uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_uploadsFolderPath))
            {
                Directory.CreateDirectory(_uploadsFolderPath);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File is required.");
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    throw new ArgumentException("File size cannot exceed 5MB.");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!Array.Exists(allowedExtensions, element => element == fileExtension))
                {
                    throw new ArgumentException("Invalid file type. Only images are allowed.");
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadsFolderPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return URL path
                var urlPath = $"/images/products/{fileName}";
                _logger.LogInformation("Image uploaded successfully: {FileName}", fileName);

                return urlPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image.");
                throw;
            }
        }

        public bool DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return false;
                }

                // Extract filename from path
                var fileName = Path.GetFileName(imagePath);
                var filePath = Path.Combine(_uploadsFolderPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Image deleted successfully: {FileName}", fileName);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image.");
                return false;
            }
        }
    }
}
