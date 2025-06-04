using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyHOADrop.Data;
using MyHOADrop.Models;
using MyHOADrop.Services;
using Microsoft.EntityFrameworkCore;

namespace MyHOADrop.Pages
{
    public class FilesIndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _storage;

        public FilesIndexModel(ApplicationDbContext db, IFileStorageService storage)
        {
            _db = db;
            _storage = storage;
        }

        // Binds the upload form (file + folder ID)
        [BindProperty]
        public UploadViewModel UploadInput { get; set; } = new();

        // Holds the files to display
        public List<FileRecord> FilesList { get; set; } = new();

        // Current folder filter (from ?folder=...)
        [BindProperty(SupportsGet = true)]
        public int? FolderFilter { get; set; }

        public void OnGet()
        {
            // Base query: all files
            var query = _db.FileRecords.AsQueryable();

            // If a folder is specified, filter by it
            if (FolderFilter.HasValue)
            {
                query = query.Where(f => f.FolderId == FolderFilter.Value);
            }

            FilesList = query
                .OrderByDescending(f => f.UploadedOn)
                .ToList();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (!ModelState.IsValid)
            {
                // If validation failed, reload the current folder’s files
                OnGet();
                return Page();
            }

            // If a folder is already selected, override the user’s typed FolderId
            if (FolderFilter.HasValue)
            {
                UploadInput.FolderId = FolderFilter.Value;
            }

            // Save file physically and get a FileRecord (UploaderId still empty)
            var record = await _storage.SaveFileAsync(UploadInput.File, UploadInput.FolderId);

            // Record the UploaderId (current user or "Anonymous")
            if (User.Identity.IsAuthenticated)
            {
                record.UploaderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? "Anonymous";
            }
            else
            {
                record.UploaderId = "Anonymous";
            }

            // Add to database
            _db.FileRecords.Add(record);
            await _db.SaveChangesAsync();

            // Set a success message in TempData so the toast appears
            TempData["ToastMessage"] = "File uploaded successfully!";

            // Redirect back to this page, preserving the folder filter
            return RedirectToPage("FilesIndex", new { folder = UploadInput.FolderId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var record = await _db.FileRecords.FindAsync(id);
            if (record != null)
            {
                // Only the uploader or an Admin can delete
                if (!User.IsInRole("Admin") &&
                    record.UploaderId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                {
                    return Forbid();
                }

                // Delete from physical storage
                _storage.DeleteFile(record);

                // Remove from DB
                _db.FileRecords.Remove(record);
                await _db.SaveChangesAsync();
            }

            // Redirect back to the same filter
            return RedirectToPage("FilesIndex", new { folder = FolderFilter });
        }
    }
}
