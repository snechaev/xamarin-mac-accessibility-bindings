# Overview

This project provides C# bindings for the macOS Accessibility API, specifically from the
[Application Services](https://developer.apple.com/documentation/applicationservices) framework and the 
`AXUIElement.h` header file. It includes elements such as the `AXUIElement` struct and the 
`AXUIElementCreateApplication(pid)` function. 
These bindings enable developers to interact with the macOS Accessibility API in a simplified and managed way, avoiding the need to work directly with low-level native APIs.

# Quick Start and Examples

## Adding the Bindings to Your Project

Currently, there is no NuGet package available. To use the bindings, follow these steps:
 
1. Download the source code.
2. Add the `AccessibilityBindings.csproj` project to your solution.
3. Reference the `AccessibilityBindings.csproj` project in your consuming project by adding the following reference:
   ```xml
   <ProjectReference Include="..\AccessibilityBindings\AccessibilityBindings.csproj" />
   ```
4. Use the API in your code (see examples below).

## Basic Usage

To interact with accessibility UI elements, follow these steps:

1. **Check and Request Permissions**  
   Ensure your application has the required accessibility permissions. If not, request the user to grant them:
   ```csharp
   if (!AccessibilityBindings.Accessibility.IsProcessTrusted(true))
   {
       // Permissions not granted. Request the user to grant them and wait for confirmation.
   }
   ```

2. **Obtain the Process ID (`pid`)**  
   Identify the process ID of the application you want to interact with. For example, locate the running application using its bundle identifier:
   ```csharp
   const string safariAppBundleId = "com.apple.Safari";
   AxUiElement safariPid = NSRunningApplication.GetRunningApplications(safariAppBundleId)
                            .FirstOrDefault()?.ProcessIdentifier;
   ```

3. **Create the Root `AXUIElement` Instance**  
   Use the `AXUIElementCreateApplication()` function to create an instance representing the application's root object:
   ```csharp
   AxUiElement axApp = AxUiElement.CreateApplication(safariPid);
   ```

4. **Traverse and Manipulate the UI Element Tree**  
   Once you have the root element, you can access and manipulate the application's UI elements:
   ```csharp
   AxUiElement mainWindow = axApp.GetMainWindow(); // Get the main window
   NSArray<AxUiElement> windows = axApp.GetWindows(); // Get all application windows
   NSArray<AxUiElement> childElements = mainWindow.GetChildren(); // Get child elements (e.g., labels, buttons)
   NSString title = mainWindow.CopyAttributeValue<NSString>(AXAttribute.Title); // Get the title attribute value
   NSString title = mainWindow.GetTitle(); // Shortcut to get the title attribute value
   ```

## Examples

1. **`SafariSample`**  
   Launches Safari, scans the UI element tree to locate the address bar, and navigates Safari to a specified URL.
Requests accessibility permissions if required.

2. **`SafariWithUiSample`**  
   Similar to `SafariSample`, but includes a full-featured UI using View, ViewController, etc. 
This demonstrates handling permission checks in applications with complex user interfaces.

3. **`PhotoshopSample`**  
   Detects specific modal dialogs (e.g., error or warning dialogs) in Adobe Photoshop and automatically clicks 
specified buttons within these dialogs by sending the `AXPress` action to the button elements.

## Debugging Notes

- If you are debugging with Rider, grant accessibility permissions to Rider, not to the application being debugged.
- When running the application outside of Rider, ensure that the application itself has the required permissions.
