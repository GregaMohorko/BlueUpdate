using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BlueUpdate.Common;
using BlueUpdate.IO;
using GM.Utility;

namespace BlueUpdate
{
	public static class Update
	{
		/// <summary>
		/// Root directory of all applications.
		/// </summary>
		public static readonly DirectoryInfo RootDirectory;

		/// <summary>
		/// Directory used for temporary files while installing/updating.
		/// </summary>
		public static readonly string TempDirectoryPath;

		/// <summary>
		/// Gets information about the currently installed BlueUpdate executable.
		/// </summary>
		public static readonly UpdatableApp Updater;

		static Update()
		{
			try {
				if(UpdatableApp.Current == null) {
					throw new InvalidOperationException("You must first set BlueUpdate.Application.Current before using the BlueUpdate.Update class.");
				}

				// check/set root folder
				string currentDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
				if(Path.GetFileName(currentDirectory) != UpdatableApp.Current.DirectoryName) {
					throw new Exception($"The folder name of the application is incorrect: '{Path.GetFileName(currentDirectory)}' should be '{UpdatableApp.Current.DirectoryName}'.");
				}
				RootDirectory = Directory.GetParent(currentDirectory);
				TempDirectoryPath = Path.Combine(RootDirectory.FullName, BlueUpdateConstants.TempDirectoryName);

				// check updater executable
				FileVersionInfo updaterFileInfo = UpdateUtility.GetUpdaterExecutableFileInfo();
				if(updaterFileInfo != null) {
					ReflectionUtility.AssemblyInformation assembly = ReflectionUtility.GetAssemblyInformation(ReflectionUtility.AssemblyType.Current);

					if(updaterFileInfo.CompanyName != assembly.Company || updaterFileInfo.ProductName != BlueUpdateConstants.UpdaterName) {
						throw new Exception("Updater executable is not legit.");
					}
					Version currentVersion = Version.Parse(updaterFileInfo.FileVersion);
					Version latestVersion = assembly.Version;

					Updater = new UpdatableApp(BlueUpdateConstants.UpdaterName, currentVersion, latestVersion, BlueUpdateConstants.UpdaterAddress, BlueUpdateConstants.UpdaterDirectoryName);
				}

				// should install/update the updater executable?
				if(Updater == null) {
					// install
					UpdatableApp updater = UpdateUtility.UpdaterAppDummy;
					UpdateUtility.Install(updater);
					Updater = updater;
				} else if(Updater.Version != Updater.VersionLatest) {
					// the version of the executable should be the same as the version of this library
					UpdateUtility.Update(Updater);
				}
			}catch(Exception e) {
				MessageBox.Show($"Error while initializing BlueUpdate:{Environment.NewLine}{Environment.NewLine}{e.Message}");
				throw e;
			}
		}
		
		/// <summary>
		/// Checks and determines whether the current application needs to update.
		/// </summary>
		public static bool Check()
		{
			return Check(UpdatableApp.Current);
		}

		/// <summary>
		/// Checks and determines whether the specified application needs to update.
		/// </summary>
		/// <param name="application">The application to check.</param>
		public static bool Check(UpdatableApp application)
		{
			return application.VersionLatest > application.Version;
		}

		/// <summary>
		/// Starts the updater and attempts to update the current application.
		/// <para>
		/// The application should be shut down right after calling this method.
		/// </para>
		/// </summary>
		/// <param name="runAfterUpdate">Determines whether to run the application after the update.</param>
		public static void Run(bool runAfterUpdate=true)
		{
			Run(UpdatableApp.Current, runAfterUpdate);
		}

		/// <summary>
		/// Starts the updater and attempts to update the specified application.
		/// <para>
		/// If the specified application is the same as the current, it should be shut down right after calling this method.
		/// </para>
		/// </summary>
		/// <param name="application">The application to update.</param>
		/// <param name="runAfterUpdate">Determines whether to run the application after the update.</param>
		public static void Run(UpdatableApp application, bool runAfterUpdate=true)
		{
			if(Updater == null) {
				throw new InvalidOperationException("The Updater was not successfully installed.");
			}

			string ignoredDirectories;
			if(application.IgnoredDirectories == null || application.IgnoredDirectories.Count==0) {
				ignoredDirectories = "{}";
			}else {
				ignoredDirectories = $"{{{string.Join(":",application.IgnoredDirectories)}}}";
			}

			// prepare the arguments
			string[] arguments = {
				application.Name,
				application.VersionLatest.ToString(),
				application.DirectoryName,
				application.Address,
				ignoredDirectories,
				runAfterUpdate.ToString()
			};

			// prepare the process
			FileInfo updaterExecutable = Files.GetExecutable(Updater);
			Process updaterProcess = new Process();
			updaterProcess.StartInfo.FileName = updaterExecutable.FullName;
			updaterProcess.StartInfo.Arguments = string.Join(" ", arguments.Select(arg => UpdateUtility.ToProcessArgument(arg)));

			// start the process
			updaterProcess.Start();
		}
	}
}
