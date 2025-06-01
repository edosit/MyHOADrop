using FluentValidation;

namespace MyHOADrop.Validators
{
    public class UploadViewModelValidator : AbstractValidator<Models.UploadViewModel>
    {
        public UploadViewModelValidator()
        {
            // 1. File must not be null
            RuleFor(x => x.File)
                .NotNull().WithMessage("Please select a file.")
                .Must(f => f.Length > 0).WithMessage("File is empty.")
                .Must(f =>
                {
                    // Limit to 10 MB
                    return f.Length <= 10 * 1024 * 1024;
                }).WithMessage("File size must be 10 MB or less.")
                .Must(f =>
                {
                    // Only allow certain extensions
                    var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                    return ext == ".pdf" || ext == ".docx" || ext == ".jpg";
                }).WithMessage("Only .pdf, .docx, or .jpg files are allowed.");

            // 2. FolderId must be positive
            RuleFor(x => x.FolderId)
                .GreaterThan(0).WithMessage("Invalid folder selected.");
        }
    }
}
