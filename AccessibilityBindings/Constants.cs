using ObjCRuntime;

namespace AccessibilityBindings;

internal static class Constants
{
    internal const string AccessibilityFramework =
        "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";

    static Constants()
    {
        var appKitLibHandle = Dlfcn.dlopen(AccessibilityFramework, 0);
        if (appKitLibHandle == IntPtr.Zero)
        {
            throw new Exception("dlopen failed to load library to get some constants");
        }

        kAXTrustedCheckOptionPrompt = GetString(appKitLibHandle, "kAXTrustedCheckOptionPrompt");
        
        Dlfcn.dlclose(appKitLibHandle);

        static NSString GetString(IntPtr libHandle, string symbol)
        {
            return Dlfcn.GetStringConstant(libHandle, symbol) ?? throw new Exception($"Can't get string constant for {symbol}");
        }

    }

    public static NSString kAXTrustedCheckOptionPrompt { get; }
}