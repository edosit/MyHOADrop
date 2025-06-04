using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MyHOADrop.Data;
using MyHOADrop.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyHOADrop.Services
{

    public interface IFileStorageService
    {
        /// <summary>
        /// Saves the uploaded file into wwwroot/Uploads/{folderId} and returns a populated FileRecord.
        /// </summary>
        Task<FileRecord> SaveFileAsync(IFormFile file, int folderId);

        /// <summary>
        /// Deletes the physical file from disk based on the FileRecord.
        /// </summary>
        void DeleteFile(FileRecord record);
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public LocalFileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<FileRecord> SaveFileAsync(IFormFile file, int folderId)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("No file provided or file is empty.");
            }

            // 1. Sanitize the incoming filename
            var untrustedFileName = Path.GetFileName(file.FileName);

            // 2. Whitelist file extensions (adjust as needed)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".txt" };
            var ext = Path.GetExtension(untrustedFileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException($"Files of type '{ext}' are not allowed.");
            }

            // 3. Generate a safe, unique filename
            var safeFileName = $"{Guid.NewGuid():N}_{untrustedFileName}";

            // 4. Build the target folder path under wwwroot/Uploads/{folderId}
            var uploadsFolderPath = Path.Combine(_env.WebRootPath, "Uploads", folderId.ToString());
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // 5. Build the full file path
            var fullFilePath = Path.Combine(uploadsFolderPath, safeFileName);

            // 6. Copy the contents
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 7. Create and return the FileRecord
            var record = new FileRecord
            {
                Filename   = safeFileName,
                UploadedOn = DateTime.UtcNow,
                Size       = file.Length,
                FolderId   = folderId,
                UploaderId = string.Empty // Caller will set this
            };

            return record;
        }

        public void DeleteFile(FileRecord record)
        {
            if (record == null)
                return;

            // Build the path: wwwroot/Uploads/{FolderId}/{Filename}
            var filePath = Path.Combine(_env.WebRootPath, "Uploads", record.FolderId.ToString(), record.Filename);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}