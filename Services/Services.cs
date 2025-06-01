using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MyHOADrop.Data;
using MyHOADrop.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyHOADrop.Services
{
    public class Services
    {
    }

    public interface IFileStorageService
    {
        Task SaveFileAsync(IFormFile file, int folderId);
        Task DeleteFileAsync(string filename, int folderId);
    }

        public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _db;

        public LocalFileStorageService(
            IWebHostEnvironment env,
            ApplicationDbContext db)
        {
            _env = env;
            _db = db;
        }

        public async Task SaveFileAsync(IFormFile file, int folderId)
        {
            // 1) Ensure the folder exists on disk: wwwroot/Uploads/{folderId}
            var uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads", folderId.ToString());
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 2) Generate a unique filename
            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            // 3) Save the file to disk
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 4) Record metadata in the database
            var record = new FileRecord
            {
                Filename = uniqueName,
                UploadedOn = DateTime.UtcNow,
                Size = file.Length,
                UploaderId = null
                /* fetch from HttpContext.User.Identity.Name or similar */,
                FolderId = folderId
            };
            _db.FileRecords.Add(record);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteFileAsync(string filename, int folderId)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads", folderId.ToString());
            var filePath = Path.Combine(uploadsFolder, filename);

            if (File.Exists(filePath))
                File.Delete(filePath);

            // Deleting the database record itself happens in the controller,
            // so this method just removes the physical file.
            await Task.CompletedTask;
        }
    }
}
