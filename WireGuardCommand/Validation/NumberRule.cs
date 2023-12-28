using System.Globalization;
using System.Windows.Controls;

namespace WireGuardCommand.Validation
{
    public class NumberRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value is null)
            {
                return new ValidationResult(false, "There was an error validating input.");
            }

            if(!int.TryParse(value as string, out int result))
            {
                return new ValidationResult(false, "You must input a number.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
