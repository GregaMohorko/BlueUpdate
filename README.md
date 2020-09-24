# BlueUpdate
A toolkit for automatic updating of .NET applications from the web. It can be used as a simple deployment tool. Includes it's own executable updater, which is automatically downloaded the first time the application starts.

[![Release](https://img.shields.io/github/release/GregaMohorko/BlueUpdate.svg?style=flat-square)](https://github.com/GregaMohorko/BlueUpdate/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/BlueUpdate.svg?style=flat-square)](https://www.nuget.org/packages/BlueUpdate)

## Usage
```C#
// 1st, define the currently running application
// you can use a method from ReflectionUtility class
// that gets the assembly information of the current application
var assembly = ReflectionUtility.GetAssemblyInformation(ReflectionUtility.AssemblyType.Application);
UpdatableApp.Current = new UpdatableApp(
  assembly.Title, // name
  assembly.Version, // current version
  latestVersion, // latest version (connect to your server to check it)
  "http://install.myapp.com" // address to the app directory on your server with the zip files
);

// now we can check and possibly update
if(Update.Check()) {
  Update.Run();
  Shutdown(); // shuts down the current application
}
```

The above example is very raw. Please read the notes below and check the recommended usage for WPF applications below.

## How it works
Please read the notes below carefully. `BlueUpdate` toolkit is not at all complicated, but it will not work if it is not set up correctly. That is why I will try to explain it in great detail.

In order to use the `BlueUpdate` toolkit, you must have your own web directory in which you will put the applications zip files. The directory hierarchy inside of it must be in the format: *[version]/[appName] [version].zip*. For example, a possible location of a zip file for the case from above could be *`http://install.myapp.com/1.0.4.2/My App 1.0.4.2.zip`*. This zip file would obviously represent the application *My App* of version *1.0.4.2*.

Before using the `Update` class, you must set the `UpdatableApp.Current`, which represents the currently running application. This must be done in order to check if the currently running application was installed correctly, to set the root directory, etc.. Every application must be inside the directory with the same name as the application itself. The recommended install path format is: *`/MyCompany/My App/My App.exe`*. The root directory of this example is *`/MyCompany`*. This way, you can have multiple applications inside your companies root directory.

When you use the `Update` class for the first time, its static initializer will check if the updater is installed inside the root directory. If I follow the previous path example, the updater would be installed in *`/MyCompany/Updater`*. If the updater is not yet installed, it will install it. The updater is very small (~398 KB).

The `Update.Check()` simply checks if the latest version of the application is greater than the current. If it is, then it needs to be updated.
The `Update.Run()` starts the updater application, which will update the currently running application. That is why the currently running application must close immediately after that, otherwise the updater will not be able to update/modify the files of the application.

You can also specify which application you want to update if you want to update some other application, in which case you don't need to close the one that is currently running.

The updater will download the latest version and update the files inside the applications folder. When it is done, it will try to run the updated application. You can modify this behavior by setting the method parameter of `Update.Run()` accordingly.

![Updater screenshot](/Documentation/Screenshots/Updater%20Screenshot.png?raw=true "Screenshot of the updater while updating 'My App' example application")

By default, the updater deletes everything inside the applications directory when it updates it, but you can specify which directories inside to leave as they are (for example *Resources*).

### Checksum check
`BlueUpdate` can also automatically do a [checksum](https://en.wikipedia.org/wiki/Checksum) check when the zip is downloaded. If you want to enable this feature, you have to add an XML file in the same directory as the zip and with the same name. If the zip file is at *`http://install.myapp.com/1.0.4.2/My App 1.0.4.2.zip`*, then the XML must be at *`http://install.myapp.com/1.0.4.2/My App 1.0.4.2.xml`*. The checksum of the zip file must be calculated using the SHA-256 algorithm.

The format inside the XML must be like this (replace the hash with the hash of your zip file):
```XML
<MyApp>
  <SHA256>3928C72D4F3296092DA892DFBC8D83B32880D1472CFFC9598C62825DB47C3B4F</SHA256>
</MyApp>
```

## Recommended usage for WPF applications
OK, so now that you understand in greater detail how the `BlueUpdate` toolkit works, I will show you the recommended usage for WPF applications. I think that the update check should only be done at the startup of the application, so I will show you how to do it.

Every WPF application has the `App.xaml` and `App.xaml.cs` files. This represents your application. By default, the `StartupUri` attribute in the `App.xaml` is set to `MainWindow.xaml`. Delete it, because you will manually show the `MainWindow` window later.

Because you deleted the `StartupUri`, the application doesn't know what to do at startup. That is why you must now override the `OnStartup` method inside the `App.xaml.cs`.

Here is an example of `OnStartup` method implementation inside the `App.xaml.cs`:

```C#
protected override void OnStartup(StartupEventArgs e)
{
  // make sure that updating is not done during development
  string dir = Directory.GetCurrentDirectory();
  if(!Debugger.IsAttached && !dir.EndsWith("Debug") && !dir.EndsWith("Release")) {
    try{
      // check the latest version
      Version latestVersion;
      // this is an example of how to connect to an example server
      // and check the version written inside the 'latest.version' file
      using(WebClient webClient = new WebClient()){
        string latest = webClient.DownloadString("http://install.myapp.com/latest.version");
        latestVersion = Version.Parse(latest);
      }
      
      // set the current application
      var assembly = ReflectionUtility.GetAssemblyInformation(ReflectionUtility.AssemblyType.Application);
      UpdatableApp.Current = new UpdatableApp(
        assembly.Title,
        assembly.Version,
        latestVersion,
        "http://install.myapp.com"
      );
      
      // check if it needs to be updated
      if(Update.Check()) {
        // start the updater ...
        Update.Run();
        // ... and close this application
        Shutdown();
        return;
      }
    } catch(WebException ex) {
      if(ex.Status == WebExceptionStatus.NameResolutionFailure) {
        MessageBox.Show("The update address that you specified does not exist.");
        Shutdown();
        return;
      } else if(ex.Status == WebExceptionStatus.ProtocolError && ex.Message == "The remote server returned an error: (530) Not logged in.") {
        MessageBox.Show("You must provide credentials for authentication.");
        Shutdown();
        return;
      }
      // there probably is no internet ...
      // do nothing
    } catch(Exception ex) {
      MessageBox.Show("Error: " + ex.Message);
      Shutdown();
      return;
    }
  }
  
  // show your main window
  try {
    MainWindow = new MainWindow();
    MainWindow.ShowDialog();
  } catch(Exception ex) {
    MessageBox.Show("Error: " + ex.Message);
  }
}
```

## Requirements
.NET Framework 4.7.1

## Author and License
Gregor Mohorko ([www.mohorko.info](https://www.mohorko.info))

Copyright (c) 2020 Gregor Mohorko

[Apache License 2.0](./LICENSE.md)
