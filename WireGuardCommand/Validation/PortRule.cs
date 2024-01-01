using System.Globalization;
using System.Windows.Controls;
using WireGuardCommand.Models;

namespace WireGuardCommand.Validation
{
    public class PortRule : ValidationRule
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

            if(result <= (int)PortRange.Min)
            {
                return new ValidationResult(false, "You must input a number higher than 0.");
            }

            if(result > (int)PortRange.Min && result < (int)PortRange.WellKnown)
            {
                return new ValidationResult(false, "Warning: This range is for well-known ports.");
            }

            if(result >= (int)PortRange.WellKnown && result < (int)PortRange.Registered)
            {
                return new ValidationResult(false, "Warning: This range is for registered ports.");
            }

            if(result > (int)PortRange.Max)
            {
                return new ValidationResult(false, "Ports need to be in the range 1 - 65535.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
