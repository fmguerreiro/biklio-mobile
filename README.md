# Trace Cross Platform App
========

The Trace App is a user tracking smartphone application for the TRACE project. 
The TRACE project (http://h2020-trace.eu) is an European initiative that aims to leverage movement tracking to promote better physical activity behaviours and improve urban city planning. 

This app is primarily targeted towards Android (4.4 KitKat+) and iOS (8.0+) and developed using Xamarin.
Xamarin (https://www.xamarin.com) is a framework that provides a unified, cross-platform API for programs written in C#.

Installation
------------

Getting the IDE:
- If you are on Windows, check the official configuration details (https://developer.xamarin.com/guides/ios/getting_started/installation/windows/) for installing Visual Studio with the Xamarin add-ons. Note that you require a separate Mac machine (can be configured to be networked from your PC) to build and test the iOS version of the app.
- If you are on Mac, install Xamarin Studio from https://www.xamarin.com/download.

You can now import the project from this repository and load it into your IDE.

Platform configurations:
- To compile for the Android version, open the Android SDK Manager and download the SDK platforms for Android 4.4.2 and up (API19+). In addition, download the latest Android SDK Tools, Android SDK Platform-tools and Android SDK Build-tools. 
Details for Windows: https://developer.xamarin.com/guides/android/getting_started/installation/windows/ 
- To run the iOS version of the app on the device (on the simulator you're good to go) you need to create a device provisioning profile. There are several steps required in order to do this. If you do not have an Apple developer program account, you can use a free provisioning profile to run the app on your device (steps shown here). Otherwise, follow the guide on the official guide to get set up (https://developer.xamarin.com/guides/ios/getting_started/installation/device_provisioning/).

Project configurations:
- On your IDE, right-click the 'Trace' library -> Options -> Build -> General and check 'Use MSBuild build engine', as well as selecting 'Current Profile: PCL 4.5 - Profile259'. Still in the library project options, switch to -> Build -> Compiler, and check 'Enable Optimizations' for both 'Configuration' panes 'Debug (Active)' and 'Release'.
- Right-click the 'Trace.Droid' -> Options -> Compiler and check 'Enable optimizations' for both configurations again. Go to -> Build -> Android Application, and make sure the 'Minimum Android version' is set to 'Android 4.4' and the 'Target Android version' is set to 'Android 6.0'.
- Right-click the 'Trace.iOS' -> Options -> Compiler and check 'Enable Optimizations' for both 'Configuration' panes 'Debug (Active)' and 'Release'. 
Go to Build -> iOS Build. For 'Debug (Active)' configuration uncheck 'Strip native debugging symbols'. If you get an error compiling the project for iOS, uncheck 'Enable incremental builds'.
Switch to 'iOS Bundle Signing' and for both configurations, make sure the signing identity and provisioning profile you created before are selected, do not use the 'automatic' or 'default' option. Also make sure that 'Custom Entitlements' has the 'Entitlements.plist' value.

Project Structure
--------

The project is composed of:
- common code: which is kept in the **Trace** library;
- platform-specific code: in a library for each supported platform, in this case **Trace.Droid** and **Trace.iOS**.

The common code library (also called PCL - Portable Class Library) includes all business code of the application. 
In addition, almost all UI is also shared code through the use of Xamarin.Forms, an abstraction library that creates native UI components, which can created in C#, XAML or a mix of the two (see Xamarin guide for more details: https://developer.xamarin.com/guides/xamarin-forms/getting-started/introduction-to-xamarin-forms/).
The starting point of the common code application is entered after each platform-specific project main (located in AppDelegate.cs (iOS) and MainActivity.cs (Android)) initializes.

For features that are specific to certain devices and for which there exists no user developed plugin (see https://github.com/xamarin/XamarinComponents for a list of the most common and popular plugin options) or for presenting specific UI elements, you need to overwrite the would-be feature with a call to platform-specific code.

In order to have access to platform-dependent code from the common code library, do: 
- create an interface class in the common code library;
- for each platform, implement the interface like so: 
```c#
[assembly: Xamarin.Forms.Dependency(typeof(YOUR_DERIVED_CLASS))]
namespace Trace {

    public class YOUR_DERIVED_CLASS : YOUR_INTERFACE {
        
        public ... YOUR_METHOD(...) { ... }
    }
}
```
- call DependencyService.Get<YOUR_INTERFACE>().YOUR_METHOD() from the common code library;

For modifying a UI element or the whole page from a Xamarin.Forms page, the process is similar (you can check this guide for details: https://developer.xamarin.com/guides/xamarin-forms/custom-renderer/contentpage/):
- create the page you want to modify in the common code library;
- for iOS, implement the renderer, overriding the OnElementChanged method which calls iOS's UIViewController, like so:
```c#
[assembly: ExportRenderer(typeof(YOUR_COMMON_CODE_PAGE), typeof(YOUR_PAGE_RENDERER))]
namespace Trace.iOS {

    public class YOUR_PAGE_RENDERER : PageRenderer {

    protected override void OnElementChanged(VisualElementChangedEventArgs e) { ... }
    }
}
```
- for Android, you do the same, this time instantiating Android's ViewGroup control, which you can then use to display native UI elements:
```c#
[assembly: ExportRenderer(typeof(YOUR_COMMON_CODE_PAGE), typeof(YOUR_PAGE_RENDERER))]
namespace Trace.Droid {

    public class YOUR_PAGE_RENDERER : PageRenderer {

        protected override void OnElementChanged (ElementChangedEventArgs<Page> e) { ... }
    }
}
```

Architecture
----------

This app is structered into several layers.
- Business layer: contains the domain model of the application. This includes Users, Trajectories, Checkpoints, Campaigns and how they relate to each other.
- Data layer: defines the data persistence of the application which includes file system access and sqlite storage.
- Service Access layer: contains the client classes that access network resources, which is mainly the Trace webserver.
- UI layer: stores pages and widgets of the application that the user will see. 
- Application layer: has application specific login that is not reusable for other projects, such as the reward eligibility state machine, background sound player and the trajectory & shop map. 

In addition there is a Security module that:
- handles user input validation in page forms;
- OAuth (Google & Facebook) provider information;
- user credentials storage (username/password or id/token from OAuth providers) that uses native device keychain or keystore to encrypt and store them on the device.

Finally, there is a Localization module that keeps classes that fetch the language from the phone, and individual Language.resx files (similar to XML) for each different language supported (Language.resx is the default English, Language.pt.resx for Portuguese, etc.).

Implementation Details
----------

UI with Xamarin.Forms:

The UI is written using XAML and C#. Pages are written mainly using the MVC pattern ... TODO



Contribute
----------

- Issue Tracker: github.com/$project/$project/issues
- Source Code: github.com/$project/$project

Support
-------

If you are having issues, please let us know.
We have a mailing list located at: project@google-groups.com

License
-------

The project is licensed under the BSD license.
