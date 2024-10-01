using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Components;

namespace WireGuardCommand.Components;

public partial class InputValidator
{
    [Parameter] 
    public required object Object { get; set; }
    
    [Parameter]
    public required string Property { get; set; }

    [Parameter]
    public required object Value { get; set; }

    private string error = "";

    protected override void OnParametersSet()
    {
        Validate();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        Validate();
    }

    private void Validate()
    {
        error = "";
        
        try
        {
            Validator.ValidateProperty(Value, new ValidationContext(Object, null, null)
            {
                MemberName = Property,
            });
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }
}