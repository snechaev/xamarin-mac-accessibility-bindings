using AccessibilityBindings;
using ScriptingBridge;

namespace SafariSample;

internal class Program
{
    public static void Main(string[] args)
    {
        NSApplication.Init();
        if (!RequestPermissionsIfNeeded())
        {
            return;
        }
        NavigateSafari("https://github.com/xamarin/xamarin-macios/issues/7536");
    }
    
    private static bool RequestPermissionsIfNeeded()
    {
        //if no permissions are needed, will show the dialog **asynchronously** (in the non-blocking way) and return false.
        if (AccessibilityBindings.Accessibility.IsProcessTrusted(true))
        {
            return true;
        }
        
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        try
        {
            //let's check in the background if user granted the permissions.
            //When granted, close the modal alert.
            //While the background check is running, show the modal alert (see below).
            Task.Run(async () =>
            {
                while(!token.IsCancellationRequested && !AccessibilityBindings.Accessibility.IsProcessTrusted())
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50), cts.Token);
                }

                if (token.IsCancellationRequested)
                {
                    return;
                }

                NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
                    NSApplication.SharedApplication.StopModal());
            }, token);
        }
        catch (TaskCanceledException)
        {
            //ignore
        }
        
        // Create a spinner (NSProgressIndicator) to show it in NSAlert. Center it in an NSView
        var spinner = new NSProgressIndicator
        {
            ControlSize = NSControlSize.Regular,
            Style = NSProgressIndicatorStyle.Spinning,
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        spinner.StartAnimation(null);

        var accessoryView = new NSView();
        accessoryView.SetContentHuggingPriorityForOrientation(1000, NSLayoutConstraintOrientation.Horizontal);
        accessoryView.SetContentHuggingPriorityForOrientation(1000, NSLayoutConstraintOrientation.Vertical);
        accessoryView.AddSubview(spinner);

        accessoryView.AddConstraints(
        [
            spinner.CenterXAnchor.ConstraintEqualTo(accessoryView.CenterXAnchor),
            spinner.CenterYAnchor.ConstraintEqualTo(accessoryView.CenterYAnchor),
        ]);

        var alert = new NSAlert
        {
            MessageText = "No accessibility permissions granted.",
            InformativeText = "When permissions will be granted, we will proceed automatically. Waiting for the permissions...",
            AccessoryView = accessoryView,
        };
            
        var cancelButton = alert.AddButton("Cancel");
        cancelButton.Tag = 1;
        cancelButton.KeyEquivalent = "\x1b";//esc

        var res = alert.RunModal();
        if (res == cancelButton.Tag)
        {
            cts.Cancel();
            return false;
        }

        return true;
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
        
        var addressInput = GetUrlInput(axApp)!;
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
