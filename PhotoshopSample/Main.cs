using AccessibilityBindings;

namespace PhotoshopSample;

internal class Program
{
    public static void Main(string[] args)
    {
        NSApplication.Init();
        if (!RequestPermissionsIfNeeded())
        {
            return;
        }

        CloseMergeErrorWindow();
        CloseUnlinkedAssetsDialog();
        ClosePreCs6Warning();
        UpdateTextLayers();
    }

    private static bool RequestPermissionsIfNeeded()
    {
        if (!AccessibilityBindings.Accessibility.IsProcessTrusted(true))
        {
            var alert = new NSAlert
            {
                MessageText = "No accessibility permissions granted.",
                InformativeText = "Grant the permissions and run again.",
                Icon = NSImage.ImageNamed(NSImageName.Caution)!,
            };
            alert.RunModal();
            return false;
        }

        return true;
    }

    private static AxUiElement? GetPhotoshopAxApplication()
    {
        var runningApps = NSWorkspace.SharedWorkspace.RunningApplications;
        var photoshopApp = runningApps.FirstOrDefault(x => x.BundleIdentifier == "com.adobe.Photoshop");
        if (photoshopApp is null)
        {
            return null;
        }

        var axApp = AxUiElement.CreateApplication(photoshopApp.ProcessIdentifier);
        return axApp;
    }

    private static IEnumerable<AxUiElement> EnumModalDialogs()
    {
        var axApp = GetPhotoshopAxApplication();
        if (axApp is null)
        {
            yield break;
        }

        // Get the app's windows
        var windowList = axApp.GetWindows();
        foreach (var window in windowList)
        {
            var title = window.GetTitle();
            if (title != "Adobe Photoshop")
            {
                continue;
            }

            var role = window.GetRole();
            if (role != "AXWindow")
            {
                continue;
            }

            var subRole = window.GetSubRole();
            if (subRole != "AXDialog")
            {
                continue;
            }

            if (!window.IsModal())
            {
                continue;
            }

            yield return window;
        }
    }

    private static void CloseUnlinkedAssetsDialog()
    {
        var expectedChildTypes = new[] { "AXButton", "AXButton", "AXScrollBar", "AXStaticText" }.Order().ToArray();
        foreach (var window in EnumModalDialogs())
        {
            //relink dialog does not have any accessible texts or something else to distinguish it from other dialogs
            //(the text is actually present in the window, but it is not accessible via the accessibility API
            //and even Apple's Accessibility Inspector does not show it)
            //So, we`ll try to guess the dialog by the checking the count and types of its children
            var children = window.GetChildren().ToArray();
            if (children.Length != 4)
            {
                return;
            }

            var childTypes = children.Select(x => x.GetRole()).Order().ToArray();
            if (!childTypes.SequenceEqual(expectedChildTypes))
            {
                return;
            }

            var defButton = window.GetDefaultButton();
            var isModal = window.IsModal();
            var availableActions = defButton.CopyActionNames();
            defButton.PerformAction("AXPress");
            return;
        }
    }


    private static void CloseMergeErrorWindow()
    {
        foreach (var window in EnumModalDialogs())
        {
            var children = window.GetChildren().ToArray();
            var childrenTypes = children.Select(x =>
            {
                var type = x.GetRole();
                return Tuple.Create(x, type);
            });

            //todo: type safe replacement for the "AXStaticText" constant?
            var textElement = childrenTypes.First(x => x.Item2 == "AXStaticText").Item1;
            var text = textElement.GetValue();
            if (text != "Команда \"Объединить группу\" не может быть выполнена: произошел программный сбой.")
            {
                continue;
            }

            var defButton = window.GetDefaultButton();

            var availableActions = defButton.CopyActionNames();
            defButton.PerformAction("AXPress");

            return;
        }
    }

    private static void ClosePreCs6Warning()
    {
        foreach (var window in EnumModalDialogs())
        {
            var children = window.GetChildren().ToArray();
            var childrenTypes = children.Select(x =>
            {
                var type = x.GetRole();
                return Tuple.Create(x, type);
            });

            //todo: type safe replacement for the "AXStaticText" constant?
            var textElement = childrenTypes.First(x => x.Item2 == "AXStaticText").Item1;
            var text = textElement.GetValue();
            if (text != "Этот документ содержит вложенные группы слоев, вид которых может измениться в результате открытия в других приложениях, выпущенных до Photoshop CS6.")
            {
                continue;
            }

            var defButton = window.GetCancelButton();
            var availableActions = defButton.CopyActionNames();
            defButton.PerformAction("AXPress");

            return;
        }
    }

    private static void UpdateTextLayers()
    {
        foreach (var window in EnumModalDialogs())
        {
            var children = window.GetChildren().ToArray();
            var childrenTypes = children.Select(x =>
            {
                var type = x.GetRole();
                return Tuple.Create(x, type);
            });

            //todo: type safe replacement for the "AXStaticText" constant?
            var textElement = childrenTypes.First(x => x.Item2 == "AXStaticText").Item1;
            var text = textElement.GetValue();
            if (text != "Некоторые текстовые слои перед векторным выводом требуется обновить. Обновить эти слои?")
            {
                continue;
            }

            var defButton = window.GetDefaultButton();
            var availableActions = defButton.CopyActionNames();
            defButton.PerformAction("AXPress");

            return;
        }
    }
}