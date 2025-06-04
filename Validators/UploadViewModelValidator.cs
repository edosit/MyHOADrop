using FluentValidation;
using MyHOADrop.Models;
using Microsoft.AspNetCore.Http;

namespace MyHOADrop.Validators
{
    public class UploadViewModelValidator : AbstractValidator<UploadViewModel>
    {
        public UploadViewModelValidator()
        {
            // 1) Ensure File is not null when you’re actually uploading.
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("Please select a file.");

            // 2) If File is non-null, check its length (this guard prevents NRE).
            RuleFor(x => x.File.Length)
                .Must(len => len > 0)
                .WithMessage("The selected file is empty.")
                .When(x => x.File != null);

            // 3) FolderId must be greater than zero (or whatever business rule you have).
            RuleFor(x => x.FolderId)
                .GreaterThan(0)
                .WithMessage("Folder ID must be greater than zero.");
        }
    }
}