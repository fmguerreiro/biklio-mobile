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
- On your IDE, right-click the 'Trace' library -> Options -> Build -> General and check 'Use MSBuild build engine', as well as selecting 'Current Profile: PCL 4.5 - Profile111'. Still in the library project options, switch to -> Build -> Compiler, and check 'Enable Optimizations' for both 'Configuration' panes 'Debug (Active)' and 'Release'.
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

This section covers a few aspects that are specific to this project that help understand the code more clearly.

### UI with Xamarin.Forms:

The UI is composed of Pages using the Model, View, Controller (MVC) pattern.
Each Page is composed of at least a XAML (an XML-based language) file (<page_name>.xaml) that specifies the design of the UI (View), and a C# "code-behind" file (<page_name>.xaml.cs) that defines the logic of the program when the page loads or when the user interacts with the Page (Controller). Optionally, there can be a Model class that logically structures the data that is used in the Page. This model must be linked to the View through a process called "Binding" (<view>.BindingContext = <model>) for the data to be displayed.

### User eligibility:

The goal of this app is to reward users that use bicycles.
Users are said to be eligible for rewards when the app detects cycling for more than a few minutes (1.5 minutes at the moment of writing). 

To implement this, we use a small state machine with 5 states: 'Ineligible', 'Cycling Ineligible', 'Cycling Eligible', 'Unknown Eligible' and 'Vehicular'.

The start state is 'Ineligible'. After a THRESHOLD number of cycling events in a row (detected from the Motion Manager from the device), the state transitions to 'Cycling Ineligible'. If the user keeps cycling for a TIME_THRESHOLD, the state transitions to 'Cycling Eligible', otherwise, it goes back to 'Ineligible'.

Once the state becomes 'Cycling Eligible', the user receives a notification and an audible warning, unlocking all the rewards that require cycling.

If the user stops cycling (a THRESHOLD number of non-cycling events occur), the state transitions to 'Unknown Eligible' (meaning the user remains eligible for rewards), which stays there until the next day (refreshed when the user goes back to 'Cycling Eligible' after a THRESHOLD number of cycling events).

On the 'Unknown Cycling' state, if there is a detection of a THRESHOLD number of vehicular events, the user is penalized, going to the 'Vehicular' state, starting a much shorter timer (around 5 minutes) that brings the user back to 'Ineligible' unless the user goes back to 'Unknown Eligible' (with a THRESHOLD number of non-vehicular events). 

### Motion detection in the background:

The central feature for the app is that it can notify the user of rewards even when she is busy doing other things. 
To do this, we need to have the app always running in the background.

Android allows this easily enough with the 'Service' interface.

In iOS however, this process is more complicated.
Apple only allows an app to do a specific set of actions to be run continuously in the background, namely:
play or record audio, track user location, use VoIP, download updates and communicate with a bluetooth accessory.
Our first option involved tracking the user location at all times. However, this makes the app consume ~10% battery/h (on iPhone 5s), which is unreasonable. 
The app still allows the user to track their trajectories, which allows the app to run continuously. We still needed another option for when the app was not tracking the user.

Other fitness apps like Runkeeper and Strava overcome this problem by instead relying on the audio category through the use of a 'personal trainer' voice that motivates the user while running. In addition, they also use the accessory category by communicating with a heart-rate monitor.

Our app uses the audio background option as well, letting the user know their eligibility state continuously.
The user may choose at any time to silence or disable this audio option.
If the user disables the audio, we lose the real-time notification ability.
iPhone devices starting from version 5s carry a dedicated processor for motion updates called M7 (M8 and M10 in later devices) which continuosly records user activity. 

Another way to get processing time in the background is by registering to radio cell tower change events or geofence regions enter/leave events.
We leverage this feature as well in case the user does not want the audio option (or Apple rejects the proposal in the App Store).
- On radio cell tower changes, we query iOS's Motion Activity Manager for the motion data that occured between radio cell change periods and update the state machine. In addition, we calculate a continuous average for the user's speed to help determine if the user is riding a bicycle. IMPORTANT: As of this moment (iOS 10.0.2), the motion data does not register Cycling events! In our tests, they always register as 'Walking'.
- We also place a geofence on each shop closest to the user (up to a maximum of 20 on iOS and 100 on Android). When the event fires, we check the average speed to see if the user is likely to be using a bicycle or walking and check whether to attribute the reward to the user.

Notes
-------

- If the iOS version fails to deploy to the device, open XCode with a project with the same bundle identifier as the Xamarin.iOS project (pt.inesc.trace), go to the Info.plist, under the 'General' tab -> 'Signing' section, make sure you select 'automatically manage signing'. If you're using 'free provising', the profile will expire every week, so you must repeat this process each time.   
- Do not update the oxyplot lib (plot drawning library) from NuGet. The current Android version adds build errors.


Support
-------

If you are having issues, please let me know.
You can reach me at gmail with: filipe.m.guerreiro

License
-------

The program is developed by INESC-ID and is part of the TRACE project, which is released under the Creative Commons license.
See file LICENSE.md for full license details.