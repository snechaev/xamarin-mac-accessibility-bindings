using System.Runtime.InteropServices;

namespace AccessibilityBindings;

public static class Accessibility
{
    //Boolean AXIsProcessTrusted(void);
    [DllImport(Constants.AccessibilityFramework)]
    [return:MarshalAs(UnmanagedType.I1)]
    private static extern bool AXIsProcessTrusted();
    
    //Boolean AXIsProcessTrustedWithOptions(CFDictionaryRef options);
    [DllImport(Constants.AccessibilityFramework)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool AXIsProcessTrustedWithOptions(IntPtr /*CFDictionaryRef*/ options);
    
    public static bool IsProcessTrusted()
    {
        return AXIsProcessTrusted();
    }
    
    public static bool IsProcessTrusted(bool askIfNeeded)
    {
        var options = NSDictionary.FromObjectAndKey(NSNumber.FromBoolean(askIfNeeded), Constants.kAXTrustedCheckOptionPrompt);
        return AXIsProcessTrustedWithOptions(options.Handle);
    }
}