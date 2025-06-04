using System;
using System.ComponentModel.DataAnnotations;

namespace MyHOADrop.Models
{
    public class FileRecord
    {
        public int Id { get; set; }

        [Required]
        public string Filename { get; set; } = default!;

        [Required]
        public string UploaderId { get; set; } = default!;

        [Required]
        public DateTime UploadedOn { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public int FolderId { get; set; }
    }
}