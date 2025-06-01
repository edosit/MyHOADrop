namespace MyHOADrop.Models
{
    public class UploadViewModel
    {
        public IFormFile File { get; set; }
        public int FolderId { get; set; }
    }

    public class FileRecord
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public string UploaderId { get; set; }
        public DateTime UploadedOn { get; set; }
        public long Size { get; set; }
        public int FolderId { get; set; }
    }

}
