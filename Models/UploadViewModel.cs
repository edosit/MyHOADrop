using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MyHOADrop.Models
{
    public class UploadViewModel
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile File { get; set; } = default!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Folder ID must be greater than zero.")]
        public int FolderId { get; set; }
    }
}

