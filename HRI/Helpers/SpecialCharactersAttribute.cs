using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HRI.Helpers
{
    public class SpecialCharactersAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value,
                            ValidationContext validationContext)
        {
            var regex = new Regex(@"[`~\!#\$%\^&\*\(\)\+\{\}\[\]]");
            if (regex.IsMatch((string)value))
            {
                return new ValidationResult("Special characters are not allowed");
            }
            return ValidationResult.Success;
        }
    }
}