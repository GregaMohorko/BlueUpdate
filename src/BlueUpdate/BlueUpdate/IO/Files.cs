using System;
using System.Collections.Generic;
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
			DirectoryInfo appDirectory = Directories.GetDirectory(application.DirectoryName, false);
			if(appDirectory == null) {
				return null;
			}

			string appExecutableName = application.Name + ".exe";
			FileInfo[] files = appDirectory.GetFiles(appExecutableName, SearchOption.TopDirectoryOnly);
			if(files.Length == 1) {
				return files[0];
			}
			if(files.Length == 0) {
				return null;
			}

			throw new Exception($"There are multiple files that are returned for the search pattern '{appExecutableName}' for application '{application.Name}'.");
		}
	}
}
