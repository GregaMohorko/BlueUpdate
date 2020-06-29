/*
MIT License

Copyright (c) 2018 Grega Mohorko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Project: BlueUpdate
Created: 2017-10-29
Author: GregaMohorko
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
				TempDirectoryPath = Path.Combine(RootDirectory.FullName, BlueUpdateConstants.TEMP_DIRECTORY_NAME);

				// check updater executable
				FileVersionInfo updaterFileInfo = UpdateUtility.GetUpdaterExecutableFileInfo();
				if(updaterFileInfo != null) {
					ReflectionUtility.AssemblyInformation assembly = ReflectionUtility.GetAssemblyInformation(ReflectionUtility.GetAssembly(ReflectionUtility.AssemblyType.CURRENT));

					if(updaterFileInfo.CompanyName != assembly.Company || updaterFileInfo.ProductName != BlueUpdateConstants.UPDATER_NAME) {
						throw new Exception("Updater executable is not legit.");
					}
					Version currentVersion = Version.Parse(updaterFileInfo.FileVersion);
					Version latestVersion = assembly.Version;

					Updater = new UpdatableApp(BlueUpdateConstants.UPDATER_NAME, currentVersion, latestVersion, BlueUpdateConstants.UPDATER_ADDRESS, BlueUpdateConstants.UPDATER_DIRECTORY_NAME);
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
		/// <param name="updaterBehavior">Determines the behavior of the updater application.</param>
		/// <param name="credentials">The network credentials that are sent to the host and used to authenticate the request.</param>
		public static void Run(UpdaterBehavior updaterBehavior=UpdaterBehavior.RUN_AFTER_UPDATE, NetworkCredential credentials = null)
		{
			Run(UpdatableApp.Current, updaterBehavior, credentials);
		}

		/// <summary>
		/// Starts the updater and attempts to update the specified application.
		/// <para>
		/// If the specified application is the same as the current, it should be shut down right after calling this method.
		/// </para>
		/// </summary>
		/// <param name="application">The application to update.</param>
		/// <param name="updaterBehavior">Determines the behavior of the updater application.</param>
		/// <param name="credentials">The network credentials that are sent to the host and used to authenticate the request.</param>
		public static void Run(UpdatableApp application, UpdaterBehavior updaterBehavior=UpdaterBehavior.RUN_AFTER_UPDATE, NetworkCredential credentials=null)
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

			string credentialsUsername = "{";
			string credentialsPassword = "{";
			string credentialsDomain = "{";
			if(credentials != null) {
				credentialsUsername += credentials.UserName;
				credentialsPassword += credentials.Password;
				credentialsDomain += credentials.Domain;
			}
			credentialsUsername += "}";
			credentialsPassword += "}";
			credentialsDomain += "}";

			string commandLineArgs = string.Join(" ", Environment.GetCommandLineArgs().Skip(1));

			// prepare the arguments
			string[] arguments = {
				application.Name,
				application.VersionLatest.ToString(),
				application.DirectoryName,
				application.Address,
				ignoredDirectories,
				updaterBehavior.ToString(),
				credentialsUsername,
				credentialsPassword,
				credentialsDomain,
				commandLineArgs
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
