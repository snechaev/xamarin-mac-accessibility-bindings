using AccessibilityBindings;
using ObjCRuntime;
using ScriptingBridge;

namespace SafariWithUiSample;

public partial class ViewController : NSViewController
{
    protected ViewController(NativeHandle handle) : base(handle)
    {
        // This constructor is required if the view controller is loaded from a xib or a storyboard.
        // Do not put any initialization here, use ViewDidLoad instead.
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Do any additional setup after loading the view.
    }

    public override NSObject RepresentedObject
    {
        get => base.RepresentedObject;
        set
        {
            base.RepresentedObject = value;

            // Update the view, if already loaded.
        }
    }



    private Task permissionsTask = Task.CompletedTask;
    private CancellationTokenSource cts = new();
   
    async partial void NavigateAction(NSObject sender)
    {
        try
        {
            var locCts = cts;
            await locCts.CancelAsync();
            locCts.Dispose();

            try
            {
                await permissionsTask;
            }
            catch (TaskCanceledException)
            {
                //ignore
            }

            var newCts = new CancellationTokenSource();
            var locPermissionsTask = permissionsTask = RequestPermissionsIfNeeded(newCts.Token);
            Interlocked.CompareExchange(ref cts, newCts, locCts);
            await locPermissionsTask;
        }
        catch (TaskCanceledException)
        {
            return;
        }

        NavigateSafari(UrlTextBox.StringValue);
    }
    
    
    private async Task RequestPermissionsIfNeeded(CancellationToken token)
    {
        //if no permissions are needed, will show the dialog **asynchronously** (in the non-blocking way) and return false.
        if (AccessibilityBindings.Accessibility.IsProcessTrusted(true))
        {
            return;
        }

        try
        {
            progressIndicator.Hidden = permissionsLabel.Hidden = false;

            //let's check in the background if user granted the permissions.
            //When granted, close the modal alert.
            //While the background check is running, show the modal alert (see below).
            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && !AccessibilityBindings.Accessibility.IsProcessTrusted())
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
                }
            }, token);
        }
        finally
        {
            progressIndicator.Hidden = permissionsLabel.Hidden = true;
        }
    }
    
    static void NavigateSafari(string url)
    {
        const string safariAppBundleId = "com.apple.Safari";
        var safariSbApp = SBApplication.GetApplication(safariAppBundleId)!;
        if (!safariSbApp.IsRunning)
        {
            safariSbApp.Activate();
        }
        
        var safariApp = NSRunningApplication.GetRunningApplications(safariAppBundleId).First();
        var axApp = AxUiElement.CreateApplication(safariApp.ProcessIdentifier);
        if (axApp is null)
        {
            return;
        }
        
        var addressInput = GetUrlInput(axApp);
        if (addressInput is null)
        {
            var alert = new NSAlert
            {
                MessageText = "Can't find URL field",
                InformativeText = "May be all Safari's windows are closed? Try to open at least one Safari window or Quit Safari. Then try again",
            };
            alert.RunModal();
            return;
        }
        addressInput.SetValue(url);
        addressInput.PerformAction("AXConfirm");
        
    }

    static AxUiElement? GetUrlInput(AxUiElement root)
    {
        try
        {
            var children = root.GetChildren();
            var addressInput = children.FirstOrDefault<AxUiElement>(x =>
                x.GetRole() == "AXTextField" && x.GetIdentifier() == "WEB_BROWSER_ADDRESS_AND_SEARCH_FIELD");
            if (addressInput is not null)
            {
                return addressInput;
            }

            foreach (var child in children)
            {
                var res = GetUrlInput(child);
                if (res is not null)
                {
                    return res;
                }
            }
        }
        catch (AXException e) when (e.Error is AXError.NoValue)
        {
            //ignore
        }
        catch(AXException e) when (e.Error is AXError.CannotComplete)
        {
            Thread.Sleep(100); //wait until main window is loaded (in case of freshly run process)
            return GetUrlInput(root);//warning: infinite recursion possible, do not use in production
        }
        return null;
    }
    
}