using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WireGuardCommand.Validation
{
    public class NoOfClientsRuleProxy : DependencyObject
    {
        public int MaxClients
        {
            get { return (int)GetValue(MaxClientsProperty); }
            set { SetValue(MaxClientsProperty, value); }
        }

        public static readonly DependencyProperty MaxClientsProperty =
            DependencyProperty.Register("MaxClients", typeof(int), typeof(NoOfClientsRuleProxy), new PropertyMetadata(default));
    }

    public class NoOfClientsRule : ValidationRule
    {
        public NoOfClientsRuleProxy Proxy { get; set; } = new NoOfClientsRuleProxy();

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value is null)
            {
                return new ValidationResult(false, "There was an error while validating input.");
            }

            int? noOfClients = null;
            if(value is string)
            {
                if(!int.TryParse(value as string, out int result))
                {
                    return new ValidationResult(false, $"You must input a number.");
                }

                noOfClients = result;
            }

            if(value is int)
            {
                noOfClients = (int)value;
            }

            if(noOfClients is null)
            {
                return new ValidationResult(false, "There was an error while validating input.");
            }

            if(noOfClients.Value > Proxy.MaxClients || noOfClients < 1)
            {
                return new ValidationResult(false, $"You must input a number between 1 - {Proxy.MaxClients}");
            }

            return ValidationResult.ValidResult;
        }
    }
}
