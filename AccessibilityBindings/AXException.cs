namespace AccessibilityBindings;

public class AXException : Exception
{
    public AXError Error { get; }

    public AXException(string message, AXError error)
        :base(message)
    {
        Error = error;
    }
}