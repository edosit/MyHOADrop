using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyHOADrop.Data;         // <-- your ApplicationDbContext lives here
using MyHOADrop.Models;       // <-- FileRecord, UploadViewModel, etc.
using MyHOADrop.Services;     // <-- IFileStorageService, if that’s where you put it
using System.Linq;
using System.Threading.Tasks;

namespace MyHOADrop.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _storage;

        public FilesController(ApplicationDbContext db, IFileStorageService storage)
        {
            _db = db;
            _storage = storage;
        }

        // GET: api/files?folderId=1
        [HttpGet]
        public IActionResult GetFiles(int folderId)
        {
            var files = _db.FileRecords
                .Where(f => f.FolderId == folderId)
                .Select(f => new
                {
                    id = f.Id,
                    filename = f.Filename,
                    uploadedOn = f.UploadedOn,
                    uploaderName = f.UploaderId,  // replace with a join if you want real display names
                    size = f.Size
                })
                .ToList();

            return Ok(new { data = files });
        }

        // POST: api/files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _storage.SaveFileAsync(model.File, model.FolderId);
            return Ok();
        }

        // DELETE: api/files/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var file = await _db.FileRecords.FindAsync(id);
            if (file == null) return NotFound();

            await _storage.DeleteFileAsync(file.Filename, file.FolderId);
            _db.FileRecords.Remove(file);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
