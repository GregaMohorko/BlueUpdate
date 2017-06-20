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
