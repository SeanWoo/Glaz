using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Glaz.Server.Data.Validators
{
    public class LoginValidatorAttribute : ValidationAttribute
    {
        private string _errorMessage = $"Неверный электронный адрес.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = (string)value;

            if (str.Contains('@'))
            {
                if (Regex.IsMatch(str, "[^@ \t\r\n]+@[^@ \t\r\n]+\\.[^@ \t\r\n]+"))
                    return ValidationResult.Success;

                return new ValidationResult(_errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
