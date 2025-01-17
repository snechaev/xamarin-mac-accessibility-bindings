using System.Runtime.InteropServices;
using CoreFoundation;
using ObjCRuntime;

namespace AccessibilityBindings;

public class AxUiElement : NativeObject
{
    [DllImport(Constants.AccessibilityFramework)]
    private static extern AXError AXUIElementCopyAttributeValue(IntPtr /* AXUIElementRef */ element, IntPtr /* CFStringRerf */ attribute, out IntPtr /* CFTypeRef  _Nullable *value */ value);

    //AXError AXUIElementSetAttributeValue(AXUIElementRef element, CFStringRef attribute, CFTypeRef value);
    [DllImport(Constants.AccessibilityFramework)]
    private static extern AXError AXUIElementSetAttributeValue(IntPtr /* AXUIElementRef */ element, IntPtr /* CFStringRerf */ attribute, IntPtr /* CFTypeRef  value */ value);
    
    [DllImport(Constants.AccessibilityFramework)]
    private static extern IntPtr /* AXUIElement */ AXUIElementCreateApplication(int /* pid_t = int */ pid);
    
    [DllImport(Constants.AccessibilityFramework)]
    private static extern AXError AXUIElementCopyActionNames(IntPtr /* AXUIElementRef */ element, out IntPtr /*CFArrayRef  _Nullable*/ names);
    
    [DllImport(Constants.AccessibilityFramework)]
    private static extern AXError AXUIElementPerformAction(IntPtr /* AXUIElementRef */ element, IntPtr /*CFStringRef*/ action);
    
    
    internal AxUiElement(NativeHandle handle, bool owns) : base(handle, owns)
    {
    }

    public static AxUiElement FromHandle(NativeHandle handle) => new(handle, true);

    public static AxUiElement? CreateApplication(int pid)
    {
        var ptr = AXUIElementCreateApplication(pid);
        return Runtime.GetINativeObject<AxUiElement>(ptr, true);
    }

    private IntPtr CopyAttributeValueImpl(string attribute)
    {
        var cfAttribute = new CFString(attribute);
        var res = AXUIElementCopyAttributeValue(Handle, cfAttribute.Handle, out var valuePtr);
        
        //todo: Should we throw on res==AXError.NoValue? May be just return null? Or we still should distinguish between cases "value present and it is null" and "no value present at all"?
        if (res != AXError.Success)
        {
            throw new AXException($"Can't get {attribute} attribute value. Error code: {res}", res);
        }

        return valuePtr;
    }

    public NativeObject? CopyAttributeValue(string attribute)
    {
        var valuePtr = CopyAttributeValueImpl(attribute);
        return Runtime.GetINativeObject<NativeObject>(valuePtr, true);
    }

    public T? CopyAttributeValue<T>(string attribute) where T : class, INativeObject
    {
        var valuePtr = CopyAttributeValueImpl(attribute);
        return Runtime.GetINativeObject<T>(valuePtr, true);
    }

    public NativeObject? CopyAttributeValue(AXAttribute attribute)
        => CopyAttributeValue(attribute.GetConstant());

    public T? CopyAttributeValue<T>(AXAttribute attribute) where T : class, INativeObject
        => CopyAttributeValue<T>(attribute.GetConstant());

    public AXError SetAttributeValue<T>(string attribute, T value) where T : class, INativeObject
    {
        var cfAttribute = new CFString(attribute);
        return AXUIElementSetAttributeValue(this.Handle, cfAttribute.Handle, value.Handle);
    }

    public AXError SetAttributeValue<T>(AXAttribute attribute, T value) where T : class, INativeObject 
        => SetAttributeValue(attribute.GetConstant(), value);
    
    public NSArray<NSString> CopyActionNames()
    {
        var res = AXUIElementCopyActionNames(Handle, out var namesPtr);
        if (res != AXError.Success)
        {
            throw new AXException("Can't get action names", res);
        }
        return Runtime.GetNSObject<NSArray<NSString>>(namesPtr, true)!;
    }

    //TODO: can we have some type-safe way to specify action names? Smart enum may be
    //  (where we have a list of standard actions as enum values, but can also use a custom string for non-standard actions)?
    public void PerformAction(string actionName)
    {
        var cfActionName = new CFString(actionName);
        var res = AXUIElementPerformAction(Handle, cfActionName.Handle);
        //todo: from the discussion block of the https://developer.apple.com/documentation/applicationservices/1462091-axuielementperformaction?language=objc
        //  "It is possible to receive the kAXErrorCannotComplete error code ... This does not necessarily mean that the function has failed, however."
        //  So, should we reconsider the error handling and not throw on AXError.CannotComplete?
        if (res != AXError.Success)
        {
            throw new AXException($"Can't perform action {actionName}. Error code: {res}", res);
        }
    }

    
    //TODO: can all these shortcut members really return null values? Or we will get either non-null valid values or "not supported/no value" exception from the underlying CopyAttributeValue logic?
    public bool IsModal() => CopyAttributeValue<NSNumber>(AXAttribute.Modal)!.BoolValue;

    public string GetTitle() => CopyAttributeValue<NSString>(AXAttribute.Title);

    public string GetRole() => CopyAttributeValue<NSString>(AXAttribute.Role);
    
    public string GetSubRole() => CopyAttributeValue<NSString>(AXAttribute.Subrole);

    public string GetIdentifier() => CopyAttributeValue<NSString>(AXAttribute.Identifier);
    
    public NSArray<AxUiElement> GetChildren() => CopyAttributeValue<NSArray<AxUiElement>>(AXAttribute.Children)!;
    
    public AxUiElement GetDefaultButton() => CopyAttributeValue<AxUiElement>(AXAttribute.DefaultButton)!;
    
    public AxUiElement GetCancelButton() => CopyAttributeValue<AxUiElement>(AXAttribute.CancelButton)!;
    
    public AxUiElement? GetMainWindow() => CopyAttributeValue<AxUiElement>(AXAttribute.MainWindow);
    
    public NSArray<AxUiElement> GetWindows() => CopyAttributeValue<NSArray<AxUiElement>>(AXAttribute.Windows)!;
    
    public string GetValue() => CopyAttributeValue<NSString>(AXAttribute.Value);
    
    public void SetValue(string value) => SetAttributeValue(AXAttribute.Value, (NSString)value);
    
}