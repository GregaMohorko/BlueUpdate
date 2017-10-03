using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BlueUpdate.Common;
using BlueUpdate.IO;
using BlueUpdate.Security.Cryptography;
using GM.Utility;

namespace BlueUpdate
{
	/// <summary>
	/// Provides utility functions for the update process. Do not use methods from this class unless you really know what you are doing.
	/// </summary>
	public static class UpdateUtility
	{
		internal static UpdatableApp UpdaterAppDummy
		{
			get
			{
				var assembly = ReflectionUtility.GetAssemblyInformation(ReflectionUtility.GetAssembly(ReflectionUtility.AssemblyType.Current));
				return new UpdatableApp(BlueUpdateConstants.UpdaterName, assembly.Version, assembly.Version, BlueUpdateConstants.UpdaterAddress, BlueUpdateConstants.UpdaterDirectoryName);
			}
		}

		internal static FileVersionInfo GetUpdaterExecutableFileInfo()
		{
			UpdatableApp updaterAppDummy = UpdaterAppDummy;
			FileInfo updaterExecutable = Files.GetExecutable(updaterAppDummy);
			if(updaterExecutable == null) {
				return null;
			}

			return FileVersionInfo.GetVersionInfo(updaterExecutable.FullName);
		}

		/// <summary>
		/// Installs the specified application.
		/// </summary>
		/// <param name="application">The application to be installed.</param>
		/// <param name="completed">If provided, the download process will be asynchronous and this action will be called upon completion.</param>
		/// <param name="progressChanged">When doing an asynchronous call, this action will be caled upon any changes of the download progress.</param>
		public static void Install(UpdatableApp application, Action<AsyncCompletedEventArgs> completed = null, Action<DownloadProgressChangedEventArgs> progressChanged = null)
		{
			Install(application, null, completed, progressChanged);
		}

		/// <summary>
		/// Installs the specified application with the specified credentials.
		/// </summary>
		/// <param name="application">The application to be installed.</param>
		/// <param name="credentials">The network credentials that are sent to the host and used to authenticate the request.</param>
		/// <param name="completed">If provided, the download process will be asynchronous and this action will be called upon completion.</param>
		/// <param name="progressChanged">When doing an asynchronous call, this action will be caled upon any changes of the download progress.</param>
		public static void Install(UpdatableApp application, ICredentials credentials, Action<AsyncCompletedEventArgs> completed = null, Action<DownloadProgressChangedEventArgs> progressChanged = null)
		{
			DirectoryInfo appDirectory = Directories.GetDirectory(application.DirectoryName, true);

			bool exceptionRethrown = false;

			Action<string> afterDownload = (downloadedZip) =>
			{
				// extract to the app directory
				try {
					ZipFile.ExtractToDirectory(downloadedZip, appDirectory.FullName);
				} catch(Exception e) {
					// something went wrong

					// delete the directory and any files inside that were possibly left from extracting the zip
					Directory.Delete(appDirectory.FullName);

					exceptionRethrown = true;
					throw e;
				} finally {
					Directories.DeleteTemp();
				}
			};

			Directories.DeleteTemp();

			try {
				// download the zip and then call the afterDownload
				if(completed == null) {
					// synchronous
					string filePath = Download(application,credentials);
					afterDownload(filePath);
				} else {
					// asynchronous
					Action<string, AsyncCompletedEventArgs> downloadCompleted = (filePath, e) =>
					{
						Exception error = null;
						try {
							if(e.Error != null) {
								throw e.Error;
							}
							if(e.Cancelled) {
								throw new WebException("The download process was cancelled.");
							}
							afterDownload(filePath);
						} catch(Exception ex) {
							if(!exceptionRethrown) {
								// temp was already deleted when the exception was rethrown
								Directories.DeleteTemp();
							}
							error = ex;
						}

						completed(new AsyncCompletedEventArgs(error, false, null));
					};
					Download(application, credentials, downloadCompleted, progressChanged);
				}
			} catch(Exception e) {
				if(!exceptionRethrown) {
					// temp was already deleted when the exception was rethrown
					Directories.DeleteTemp();
				}
				throw e;
			}
		}

		/// <summary>
		/// Updates the specified application.
		/// </summary>
		/// <param name="application">The application to be updated.</param>
		/// <param name="completed">If provided, the download process will be asynchronous and this action will be called upon completion.</param>
		/// <param name="progressChanged">When doing an asynchronous call, this action will be caled upon any changes of the download progress.</param>
		public static void Update(UpdatableApp application, Action<AsyncCompletedEventArgs> completed = null, Action<DownloadProgressChangedEventArgs> progressChanged = null)
		{
			Update(application, null, completed, progressChanged);
		}

		/// <summary>
		/// Updates the specified application with the specified credentials.
		/// </summary>
		/// <param name="application">The application to be updated.</param>
		/// <param name="credentials">The network credentials that are sent to the host and used to authenticate the request.</param>
		/// <param name="completed">If provided, the download process will be asynchronous and this action will be called upon completion.</param>
		/// <param name="progressChanged">When doing an asynchronous call, this action will be caled upon any changes of the download progress.</param>
		public static void Update(UpdatableApp application, ICredentials credentials, Action<AsyncCompletedEventArgs> completed = null, Action<DownloadProgressChangedEventArgs> progressChanged = null)
		{
			DirectoryInfo appDirectory = Directories.GetDirectory(application.DirectoryName, true);
			string backupSuffix = BlueUpdateConstants.BackupSuffix;

			bool exceptionRethrown = false;

			Action<string> afterDownload = (downloadedZip) =>
			{
				// add the backup suffix to all current files
				IOUtility.AddSuffixToAll(appDirectory, backupSuffix, SearchOption.TopDirectoryOnly, application.IgnoredDirectories);

				// extract the downloaded zip file to the app directory
				try {
					ZipFile.ExtractToDirectory(downloadedZip, appDirectory.FullName);
				} catch(Exception e) {
					// something went wrong

					// reroll back:
					// delete all non-backup files that were possibly left from extracting the zip
					IOUtility.DeleteAllWithoutSuffix(appDirectory, backupSuffix, SearchOption.TopDirectoryOnly, application.IgnoredDirectories);
					// reverse the backup files
					IOUtility.RemoveSuffixFromAll(appDirectory, backupSuffix, SearchOption.TopDirectoryOnly);

					exceptionRethrown = true;
					throw e;
				} finally {
					// delete the backup files
					IOUtility.DeleteAllWithSuffix(appDirectory, backupSuffix, SearchOption.TopDirectoryOnly);

					// delete the temp folder
					Directories.DeleteTemp();
				}
			};

			Directories.DeleteTemp();
			try {
				// clear any backup files that possibly remained from previous update attempts
				IOUtility.DeleteAllWithSuffix(appDirectory, backupSuffix, SearchOption.TopDirectoryOnly);

				// download the zip and then call the afterDownload
				if(completed == null) {
					// synchronous
					string filePath = Download(application,credentials);
					afterDownload(filePath);
				} else {
					// asynchronous
					Action<string, AsyncCompletedEventArgs> downloadCompleted = (filePath, e) =>
					{
						Exception error = null;
						try {
							if(e.Error != null) {
								throw e.Error;
							}
							if(e.Cancelled) {
								throw new WebException("The download process was cancelled.");
							}
							afterDownload(filePath);
						} catch(Exception ex) {
							if(!exceptionRethrown) {
								// temp was already deleted when the exception was rethrown
								Directories.DeleteTemp();
							}
							error = ex;
						}

						completed(new AsyncCompletedEventArgs(error, false, null));
					};
					Download(application, credentials, downloadCompleted, progressChanged);
				}
			} catch(Exception e) {
				if(!exceptionRethrown) {
					// temp was already deleted when the exception was rethrown
					Directories.DeleteTemp();
				}
				throw e;
			}
		}

		/// <summary>
		/// Prepares the string so that it can be used as an argument for a process.
		/// </summary>
		/// <param name="argument">The text to be prepared.</param>
		public static string ToProcessArgument(string argument)
		{
			return argument.Replace(" ", "%20");
		}

		/// <summary>
		/// Creates the actual text from the process argument.
		/// </summary>
		/// <param name="argument">The argument to be converted to the actual text.</param>
		public static string FromProcessArgument(string argument)
		{
			return argument.Replace("%20", " ");
		}

		/// <summary>
		/// Downloads the specified application to the temporary folder and returns the file path of the downloaded zip.
		/// </summary>
		private static string Download(UpdatableApp application, ICredentials credentials, Action<string, AsyncCompletedEventArgs> completed = null, Action<DownloadProgressChangedEventArgs> progressChanged=null)
		{
			string fileName = $"{application.Name} {application.VersionLatest}";
			string zipFileName = $"{fileName}.zip";
			string xmlFileName = $"{fileName}.xml";
			string downloadDirectoryAddress = string.Format("{0}/{1}", application.Address, application.VersionLatest);
			string downloadFileAddress = string.Format("{0}/{1}", downloadDirectoryAddress, zipFileName);
			string downloadXMLAddress = string.Format("{0}/{1}", downloadDirectoryAddress, xmlFileName);
			DirectoryInfo tempDirectory = Directories.GetDirectory(BlueUpdateConstants.TempDirectoryName, true);
			string downloadFilePath = Path.Combine(tempDirectory.FullName, zipFileName);
			
			Action afterDownload = delegate
			{
				// download checksum if it exists
				XElement xml;
				try {
					xml = XMLUtility.LoadFromWeb(downloadXMLAddress, credentials);
				}catch(WebException) {
					// no xml with checksum is present
					return;
				}

				var sha256 = xml.Element("SHA256");
				if(sha256 == null) {
					throw new Exception("No SHA256 element inside the XML could be found.");
				}

				string statedChecksum = sha256.Value;

				// get checksum of the downloaded file
				string fileChecksum = Sha256.GetChecksum(downloadFilePath);

				if(statedChecksum != fileChecksum) {
					throw new Exception("Checksum of the downloaded zip file did not match the one specified in the XML file.");
				}
			};

			// download file to the temp folder
			using(WebClient webClient=new WebClient()) {
				if(credentials != null) {
					webClient.Credentials = credentials;
				}

				if(completed==null) {
					// synchronous call
					webClient.DownloadFile(downloadFileAddress, downloadFilePath);
					afterDownload();
					return downloadFilePath;
				} else {
					// asynchronous call
					if(progressChanged != null) {
						webClient.DownloadProgressChanged += (sender, e) =>
						{
							progressChanged(e);
						};
					}
					webClient.DownloadFileCompleted += (sender, e) =>
					{
						if(!e.Cancelled && e.Error == null) {
							try {
								afterDownload();
							} catch(Exception ex) {
								downloadFilePath = null;
								e = new AsyncCompletedEventArgs(ex, false, e.UserState);
							}
						}else {
							downloadFilePath = null;
						}

						completed(downloadFilePath, e);
					};
					webClient.DownloadFileAsync(new Uri(downloadFileAddress), downloadFilePath);
					return null;
				}
			}
		}
	}
}
