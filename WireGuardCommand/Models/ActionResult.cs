namespace WireGuardCommand.Models;

public class ActionResult
{
    public bool Success { get; }
    public string Message { get; }

    public ActionResult(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public static ActionResult Fail(string message = "")
    {
        return new ActionResult(false, message);
    }

    public static ActionResult Pass(string message = "")
    {
        return new ActionResult(true, message);
    }
}

public class ActionResult<T> : ActionResult
{
    public T? Result { get; }

    public ActionResult(bool success, string message = "", T? result = default) : base(success, message)
    {
        Result = result;
    }
}
