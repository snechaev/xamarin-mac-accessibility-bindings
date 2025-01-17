// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace SafariWithUiSample
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSTextField permissionsLabel { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator progressIndicator { get; set; }

		[Outlet]
		AppKit.NSTextField UrlTextBox { get; set; }

		[Action ("NavigateAction:")]
		partial void NavigateAction (Foundation.NSObject sender);

		void ReleaseDesignerOutlets ()
		{
			if (progressIndicator != null) {
				progressIndicator.Dispose ();
				progressIndicator = null;
			}

			if (permissionsLabel != null) {
				permissionsLabel.Dispose ();
				permissionsLabel = null;
			}

			if (UrlTextBox != null) {
				UrlTextBox.Dispose ();
				UrlTextBox = null;
			}

		}
	}
}
