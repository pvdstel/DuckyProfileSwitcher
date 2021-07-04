using System.Globalization;
using System.Windows.Controls;

namespace DuckyProfileSwitcher.Validators
{
    class RangeValidation : ValidationRule
    {
        public double Minimum { get; set; }

        public double Maximum { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is int integer && integer >= Minimum && integer <= Maximum)
            {
                return new ValidationResult(true, null);
            }
            else if (value is double dfp && dfp >= Minimum && dfp <= Maximum)
            {
                return new ValidationResult(true, null);
            }
            else
            {
                return new ValidationResult(false, $"This value must be at least {Minimum} and at most {Maximum}.");
            }
        }
    }
}
