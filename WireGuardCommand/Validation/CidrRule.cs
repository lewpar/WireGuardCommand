using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WireGuardCommand.Validation
{
    public class CidrRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string? input = value as string;

            if (input is null)
            {
                return new ValidationResult(false, "There was an error while validating input.");
            }

            if (value is not string)
            {
                return new ValidationResult(false, "There was an error while validating input.");
            }

            string[] parts = input.Split('/');

            if(parts.Length < 2)
            {
                return new ValidationResult(false, "Invalid cidr notation.");
            }

            if (!Regex.Match(parts[0], "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}").Success)
            {
                return new ValidationResult(false, "Invalid address part.");
            }

            if (!int.TryParse(parts[1], out int cidr))
            {
                return new ValidationResult(false, "Invalid cidr part.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
