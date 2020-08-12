using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Glaz.Server.Data.ValidationAttributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            if (!(value is IFormFile file))
            {
                return ValidationResult.Success;
            }

            if (file.Length <= _maxFileSize)
            {
                return ValidationResult.Success;
            }

            const int bytesInOneMebiByte = 1_048_576;
            var maxFileSizeInMebiBytes = _maxFileSize / bytesInOneMebiByte;
            return new ValidationResult($"Максимальный допустимый вес файла это {maxFileSizeInMebiBytes} MB");

        }

        public string GetErrorMessage()
        {
            return $"Maximum allowed file size is { _maxFileSize} bytes.";
        }
    }
}