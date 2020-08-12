using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Glaz.Server.Data.ValidationAttributes
{
    public class AllowedExtensionsAttribute:ValidationAttribute
    {
        private readonly List<string> _extensions;
        public AllowedExtensionsAttribute(params string[] extensions)
        {
            _extensions = extensions.ToList();
        }

        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            if (!(value is IFormFile file))
            {
                return ValidationResult.Success;
            }
            
            var extension = Path.GetExtension(file.FileName);
            if (extension is null)
            {
                return new ValidationResult("У файла отсутствует его расширение");
            }

            if (_extensions.Contains(extension.ToLower()))
            {
                return ValidationResult.Success;
            }

            var allowedExtensions = string.Join(", ", _extensions);
            return new ValidationResult($"Файл загружен неверного формата. Доступные форматы: {allowedExtensions}");
        }
    }
}