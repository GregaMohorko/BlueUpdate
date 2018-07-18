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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueUpdate.IO
{
	/// <summary>
	/// Provides utility functions for working with directories.
	/// </summary>
	public static class Directories
	{
		/// <summary>
		/// Gets the directory of the specified application.
		/// </summary>
		/// <param name="app">The application of which to get the directory.</param>
		public static DirectoryInfo GetDirectory(UpdatableApp app)
		{
			return GetDirectory(app.DirectoryName, false);
		}

		internal static DirectoryInfo GetDirectory(string name,bool createIfNotPresent=false)
		{
			DirectoryInfo[] siblingDirectories = Update.RootDirectory.GetDirectories(name, SearchOption.TopDirectoryOnly);

			if(siblingDirectories.Length == 1) {
				return siblingDirectories[0];
			}

			if(siblingDirectories.Length > 1) {
				// more than 1 folder for one application?
				throw new Exception($"There are multiple directories that are returned for the search pattern '{name}'.");
			}

			if(!createIfNotPresent) {
				return null;
			}

			return Directory.CreateDirectory(Path.Combine(Update.RootDirectory.FullName, name));
		}

		/// <summary>
		/// Deletes the BlueUpdate temporary folder.
		/// </summary>
		public static void DeleteTemp()
		{
			if(Directory.Exists(Update.TempDirectoryPath)) {
				Directory.Delete(Update.TempDirectoryPath, true);
			}
		}
	}
}
