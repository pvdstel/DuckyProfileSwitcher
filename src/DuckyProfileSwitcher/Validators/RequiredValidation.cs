using System.Globalization;
using System.Windows.Controls;

namespace DuckyProfileSwitcher.Validators
{
    internal class RequiredValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string str && !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str))
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, "This field is required.");
            }
        }
    }
}
