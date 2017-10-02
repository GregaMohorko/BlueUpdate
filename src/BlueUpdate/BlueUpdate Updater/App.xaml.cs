using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using BlueUpdate;
using BlueUpdate.Common;
using BlueUpdate.IO;
using BlueUpdate_Updater.Presentation;
using GM.Utility;

namespace BlueUpdate_Updater
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown;

			UpdatableApp appToUpdate;
			UpdaterBehavior updaterBehavior=UpdaterBehavior.SHOW_MESSAGES;
			string arguments = null;

			try {
				// set the current application as the updater
				var assembly =ReflectionUtility.GetAssemblyInformation(ReflectionUtility.GetAssembly(ReflectionUtility.AssemblyType.Application));
				UpdatableApp.Current = new UpdatableApp(BlueUpdateConstants.UpdaterName, assembly.Version, null, null, BlueUpdateConstants.UpdaterDirectoryName);
				
				// check arguments
				if(e.Args.Length < 6 || e.Args.Length > 7) {
					throw new Exception("Code 10.");
				}

				if(!Enum.TryParse(e.Args[5], out updaterBehavior)) {
					throw new Exception("Code 20.");
				}

				appToUpdate = AppFromArgs(e);

				if(appToUpdate.Name == "Updater") {
					throw new Exception("Application must not be named 'Updater'.");
				}

				if(e.Args.Length == 7) {
					arguments = UpdateUtility.FromProcessArgument(e.Args[6]);
				}
			} catch(Exception ex) {
				HandleError("Error", ex, updaterBehavior);
				Shutdown();
				return;
			}

			try {
				// show updater window and wait for it to close
				MainWindow mainWindow = new MainWindow(appToUpdate);
				mainWindow.ShowDialog();
				if(mainWindow.Error != null) {
					throw mainWindow.Error;
				}
			}catch(WebException ex) {
				HandleError($"There was a problem with the internet connection while updating {appToUpdate.Name}", ex, updaterBehavior);
				Shutdown();
				return;
			}catch(Exception ex) {
				HandleError($"There was an error while updating {appToUpdate.Name}", ex, updaterBehavior);
				Shutdown();
				return;
			}

			switch(updaterBehavior) {
				case UpdaterBehavior.SHOW_MESSAGES:
					MessageBox.Show($"{appToUpdate.Name} was successfully updated!", "Update finished");
					break;
				case UpdaterBehavior.RUN_AFTER_UPDATE:
					// startup the updated application
					try {
						FileInfo executable = Files.GetExecutable(appToUpdate);
						if(executable == null) {
							throw new Exception($"The executable file '{appToUpdate.Name}.exe' could not be found.");
						}

						// prepare the process
						var process = new Process();
						process.StartInfo.FileName = executable.FullName;
						process.StartInfo.Arguments = arguments;

						// start the process
						process.Start();
					} catch(Exception ex) {
						HandleError($"There was an error while trying to run the updated version of {appToUpdate.Name}", ex, updaterBehavior);
					}
					break;
			}

			Shutdown();
		}

		private void HandleError(string initialMessage,Exception error,UpdaterBehavior updaterBehavior)
		{
			if(updaterBehavior == UpdaterBehavior.HIDDEN) {
				return;
			}

			string message = $"{initialMessage}:";
			while(error != null) {
				message += $"{Environment.NewLine}{Environment.NewLine}{error.Message}";
				error = error.InnerException;
			}
			MessageBox.Show(message, "Error");
		}

		private UpdatableApp AppFromArgs(StartupEventArgs e)
		{
			string name;
			Version latestVersion;
			string directoryName;
			string address;
			string[] ignoredDirectories;
			{
				name = UpdateUtility.FromProcessArgument(e.Args[0]);
				if(!Version.TryParse(UpdateUtility.FromProcessArgument(e.Args[1]), out latestVersion)) {
					throw new Exception("Code 30.");
				}
				directoryName = UpdateUtility.FromProcessArgument(e.Args[2]);
				address = UpdateUtility.FromProcessArgument(e.Args[3]);
				
				// ignored directories format: {dir1:dir2:...:dirn}
				if(!e.Args[4].StartsWith("{")) {
					throw new Exception("Code 40");
				}
				if(!e.Args[4].EndsWith("}")) {
					throw new Exception("Code 50");
				}
				string inside = e.Args[4].Substring(1, e.Args[4].Length - 2);
				ignoredDirectories = (inside == string.Empty) ? null : inside.Split(':');
			}

			return new UpdatableApp(name, null, latestVersion, address, directoryName,ignoredDirectories);
		}
	}
}
