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
using System.Text;
using System.Threading.Tasks;

namespace BlueUpdate.IO
{
	/// <summary>
	/// Provides utility functions for working with files.
	/// </summary>
	public static class Files
	{
		/// <summary>
		/// Gets the executable file of the specified application.
		/// </summary>
		/// <param name="application">The application of which to get the executable file.</param>
		public static FileInfo GetExecutable(UpdatableApp application)
		{
			return GetExecutable(application.Name, application.DirectoryName);
		}

		/// <summary>
		/// Gets the executable file of the application with the specified name.
		/// </summary>
		/// <param name="applicationName">The name of the application of which to get the executable file.</param>
		/// <param name="directoryName">The name of the applications directory. If null, it is set to the name of the application.</param>
		public static FileInfo GetExecutable(string applicationName,string directoryName=null)
		{
			if(directoryName == null) {
				directoryName = applicationName;
			}

			DirectoryInfo appDirectory = Directories.GetDirectory(directoryName, false);
			if(appDirectory == null) {
				return null;
			}

			string appExecutableName = applicationName + ".exe";
			FileInfo[] files = appDirectory.GetFiles(appExecutableName, SearchOption.TopDirectoryOnly);
			if(files.Length == 1) {
				return files[0];
			}
			if(files.Length == 0) {
				return null;
			}

			throw new Exception($"There are multiple files that are returned for the search pattern '{appExecutableName}' for application '{applicationName}'.");
		}

		/// <summary>
		/// Gets the file version info for the specified application.
		/// </summary>
		/// <param name="application">The application of which to get the file version info.</param>
		public static FileVersionInfo GetFileVersionInfo(UpdatableApp application)
		{
			return GetFileVersionInfo(application.Name, application.DirectoryName);
		}

		/// <summary>
		/// Gets the file version info for the specified application.
		/// </summary>
		/// <param name="applicationName">The name of the application of which to get the file version info.</param>
		/// <param name="directoryName">The name of the applications directory. If null, it is set to the name of the application.</param>
		public static FileVersionInfo GetFileVersionInfo(string applicationName, string directoryName=null)
		{
			FileInfo fileInfo = GetExecutable(applicationName, directoryName);
			if(fileInfo == null) {
				return null;
			}

			return GetFileVersionInfo(fileInfo);
		}

		/// <summary>
		/// Gets the file version info for the specified file.
		/// </summary>
		/// <param name="fileInfo">The FileInfo object that represents the file.</param>
		public static FileVersionInfo GetFileVersionInfo(FileInfo fileInfo)
		{
			return FileVersionInfo.GetVersionInfo(fileInfo.FullName);
		}
	}
}
