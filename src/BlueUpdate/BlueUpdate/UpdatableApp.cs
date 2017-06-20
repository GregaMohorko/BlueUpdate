using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueUpdate
{
	/// <summary>
	/// Represents an application that can be updated.
	/// </summary>
	public class UpdatableApp
	{
		/// <summary>
		/// Gets or sets the currently running application.
		/// </summary>
		public static UpdatableApp Current
		{
			get { return _current; }
			set
			{
				if(_current != null) {
					throw new InvalidOperationException("Current application is already set.");
				}
				_current = value;
			}
		}
		private static UpdatableApp _current;

		/// <summary>
		/// Gets the name of this application.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the current version of this application.
		/// </summary>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the latest version of this application.
		/// </summary>
		public Version VersionLatest { get; private set; }

		/// <summary>
		/// Gets the name of this applications directory in the root directory.
		/// </summary>
		public string DirectoryName { get; private set; }

		/// <summary>
		/// Gets the web address of the directory where this application can be downloaded.
		/// </summary>
		public string Address { get; private set; }

		/// <summary>
		/// A collection of directories inside this applications directory to ignore while updating (to be left as they are).
		/// </summary>
		public ReadOnlyCollection<string> IgnoredDirectories { get; private set; }

		/// <summary>
		/// Creates a new instance of updatable application.
		/// </summary>
		/// <param name="name">The name of the application.</param>
		/// <param name="version">The current version of the application.</param>
		/// <param name="latestVersion">The latest version of the application. You should connect to the internet to check the latest version.</param>
		/// <param name="address">The web address of the directory where this application can be downloaded.</param>
		/// <param name="directoryName">The name of the applications directory in the root directory. If null, will be set to the same value as the name of the application.</param>
		/// <param name="ignoredDirectories">A collection of directories inside this applications directory to ignore while updating (to be left as they are).</param>
		public UpdatableApp(string name,Version version,Version latestVersion,string address,string directoryName=null,IEnumerable<string> ignoredDirectories=null)
		{
			Name = name;
			Version = version;
			VersionLatest = latestVersion;
			Address = address;
			DirectoryName = directoryName ?? Name;
			IgnoredDirectories = ignoredDirectories?.ToList().AsReadOnly();
		}
	}
}
