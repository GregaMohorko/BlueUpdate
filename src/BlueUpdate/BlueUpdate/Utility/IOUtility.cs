﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueUpdate.Utility
{
	public static class IOUtility
	{
		/// <summary>
		/// Adds the specified suffix to all directories and files in the specified directory, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all directories and files.</param>
		/// <param name="suffix">The suffix to add to all directories and files.</param>
		public static void AddSuffixToAll(DirectoryInfo directory, string suffix)
		{
			AddSuffixToAll(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Adds the specified suffix to all directories and files in the specified directory, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all directories and files.</param>
		/// <param name="suffix">The suffix to add to all directories and files.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void AddSuffixToAll(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			AddSuffixToAll(directory, suffix, searchOption, null);
		}

		/// <summary>
		/// Adds the specified suffix to all directories and files in the specified directory, using the specified search option. If a directory matches any of the values in the provided ignore list, the suffix will not be added to it.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all directories and files.</param>
		/// <param name="suffix">The suffix to add to all directories and files.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		/// <param name="ignoredDirectories">A collection of directory names to ignore when adding suffix.</param>
		public static void AddSuffixToAll(DirectoryInfo directory,string suffix,SearchOption searchOption,IEnumerable<string> ignoredDirectories)
		{
			AddSuffixToAllFiles(directory, suffix, SearchOption.TopDirectoryOnly);
			AddSuffixToAllDirectories(directory, suffix, SearchOption.TopDirectoryOnly,ignoredDirectories);

			if(searchOption == SearchOption.AllDirectories) {
				DirectoryInfo[] allSubdirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
				foreach(DirectoryInfo subdirectory in allSubdirectories) {
					AddSuffixToAll(subdirectory, suffix, SearchOption.AllDirectories,ignoredDirectories);
				}
			}
		}

		/// <summary>
		/// Adds the specified suffix to all directories in the specified directory, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all directories.</param>
		/// <param name="suffix">The suffix to add to all directories.</param>
		public static void AddSuffixToAllDirectories(DirectoryInfo directory, string suffix)
		{
			AddSuffixToAllDirectories(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Adds the specified suffix to all directories in the specified directory, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all directories.</param>
		/// <param name="suffix">The suffix to add to all directories.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void AddSuffixToAllDirectories(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			AddSuffixToAllDirectories(directory, suffix, searchOption, null);
		}

		/// <summary>
		/// Adds the specified suffix to all directories in the specified directory, using the specified search option. If a directory matches any of the values in the provided ignore list, the suffix will not be added to it.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all directories.</param>
		/// <param name="suffix">The suffix to add to all directories.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		/// <param name="ignoredDirectories">A collection of directory names to ignore when adding suffix.</param>
		public static void AddSuffixToAllDirectories(DirectoryInfo directory, string suffix, SearchOption searchOption, IEnumerable<string> ignoredDirectories)
		{
			DirectoryInfo[] allDirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);

			foreach(DirectoryInfo subdirectory in allDirectories) {
				if(ignoredDirectories!=null && ignoredDirectories.Contains(subdirectory.Name)) {
					continue;
				}

				if(searchOption == SearchOption.AllDirectories) {
					AddSuffixToAllDirectories(subdirectory, suffix, SearchOption.AllDirectories,ignoredDirectories);
				}
				AddSuffix(subdirectory, suffix);
			}
		}

		/// <summary>
		/// Adds the specified suffix to all files inside the specified directory, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all files.</param>
		/// <param name="suffix">The suffix to add to all files.</param>
		public static void AddSuffixToAllFiles(DirectoryInfo directory, string suffix)
		{
			AddSuffixToAllFiles(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Adds the specified suffix to all files inside the specified directory, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to add the suffix to all files.</param>
		/// <param name="suffix">The suffix to add to all files.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void AddSuffixToAllFiles(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			FileInfo[] allFiles = directory.GetFiles("*", searchOption);
			foreach(FileInfo file in allFiles) {
				AddSuffix(file, suffix);
			}
		}

		/// <summary>
		/// Renames the specified directory (adds the specified suffix to the end of the name).
		/// </summary>
		/// <param name="directory">The directory to add the suffix to.</param>
		/// <param name="suffix">The suffix to add to the directory.</param>
		public static void AddSuffix(DirectoryInfo directory, string suffix)
		{
			string newName = directory.Name + suffix;
			string absolutePath = Path.Combine(Path.GetDirectoryName(directory.FullName), newName);
			directory.MoveTo(absolutePath);
		}

		/// <summary>
		/// Renames the specified file (adds the specified suffix to the end of the name).
		/// </summary>
		/// <param name="file">The file to add the suffix to.</param>
		/// <param name="suffix">The suffix to add to the file.</param>
		public static void AddSuffix(FileInfo file, string suffix)
		{
			string newName = file.Name + suffix;
			string absolutePath = Path.Combine(Path.GetDirectoryName(file.FullName), newName);
			file.MoveTo(absolutePath);
		}

		/// <summary>
		/// Removes the specified suffix from all directories and files in the specified directory, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to remove the suffix from all directories and files.</param>
		/// <param name="suffix">The suffix to remove from all directories and files.</param>
		public static void RemoveSuffixFromAll(DirectoryInfo directory, string suffix)
		{
			RemoveSuffixFromAll(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Removes the specified suffix from all directories and files in the specified directory, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to remove the suffix from all directories and files.</param>
		/// <param name="suffix">The suffix to remove from all directories and files.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void RemoveSuffixFromAll(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			RemoveSuffixFromAllFiles(directory, suffix, SearchOption.TopDirectoryOnly);
			RemoveSuffixFromAllDirectories(directory, suffix, SearchOption.TopDirectoryOnly);

			if(searchOption == SearchOption.AllDirectories) {
				DirectoryInfo[] allSubdirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
				foreach(DirectoryInfo subdirectory in allSubdirectories) {
					RemoveSuffixFromAll(subdirectory, suffix, SearchOption.AllDirectories);
				}
			}
		}

		/// <summary>
		/// Removes the specified suffix from all directories in the specified directory, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to remove the suffix from all directories.</param>
		/// <param name="suffix">The suffix to remove from all directories.</param>
		public static void RemoveSuffixFromAllDirectories(DirectoryInfo directory, string suffix)
		{
			RemoveSuffixFromAllDirectories(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Removes the specified suffix from all directories in the specified directory, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to remove the suffix from all directories.</param>
		/// <param name="suffix">The suffix to remove from all directories.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void RemoveSuffixFromAllDirectories(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			DirectoryInfo[] allDirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);

			foreach(DirectoryInfo subdirectory in allDirectories) {
				if(searchOption == SearchOption.AllDirectories) {
					RemoveSuffixFromAllDirectories(subdirectory, suffix, SearchOption.AllDirectories);
				}
				RemoveSuffix(subdirectory, suffix);
			}
		}

		/// <summary>
		/// Removes the specified suffix from all files inside the specified directory, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to remove the suffix from all directories.</param>
		/// <param name="suffix">The suffix to remove from all directories.</param>
		public static void RemoveSuffixFromAllFiles(DirectoryInfo directory, string suffix)
		{
			RemoveSuffixFromAllFiles(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Removes the specified suffix from all files inside the specified directory, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to remove the suffix from all directories.</param>
		/// <param name="suffix">The suffix to remove from all directories.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void RemoveSuffixFromAllFiles(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			FileInfo[] allFiles = directory.GetFiles("*"+suffix, searchOption);
			foreach(FileInfo file in allFiles) {
				RemoveSuffix(file, suffix);
			}
		}

		/// <summary>
		/// Renames the specified directory (removes the specified suffix from the end of the name, if present).
		/// </summary>
		/// <param name="directory">The directory to remove the suffix from.</param>
		/// <param name="suffix">The suffix to remove from the directory.</param>
		public static void RemoveSuffix(DirectoryInfo directory, string suffix)
		{
			if(!directory.Name.EndsWith(suffix)) {
				return;
			}

			string newName = directory.Name.Substring(0, directory.Name.Length - suffix.Length);
			string absolutePath = Path.Combine(Path.GetDirectoryName(directory.FullName), newName);
			directory.MoveTo(absolutePath);
		}

		/// <summary>
		/// Renames the specified file (removes the specified suffix from the end of the name, if present).
		/// </summary>
		/// <param name="file">The file to remove the suffix from.</param>
		/// <param name="suffix">The suffix to remove from the file.</param>
		public static void RemoveSuffix(FileInfo file, string suffix)
		{
			if(!file.Name.EndsWith(suffix)) {
				return;
			}

			string newName = file.Name.Substring(0,file.Name.Length-suffix.Length);
			string absolutePath = Path.Combine(Path.GetDirectoryName(file.FullName), newName);
			file.MoveTo(absolutePath);
		}

		/// <summary>
		/// Deletes all files and directories in the specified directory, whose name is ended by the specified suffix, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files and directories.</param>
		/// <param name="suffix">The suffix that files and directories must contain in order to be deleted.</param>
		public static void DeleteAllWithSuffix(DirectoryInfo directory, string suffix)
		{
			DeleteAllWithSuffix(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Deletes all files and directories in the specified directory, whose name is ended by the specified suffix, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files and directories.</param>
		/// <param name="suffix">The suffix that files and directories must contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void DeleteAllWithSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			DeleteAllFilesWithSuffix(directory, suffix, SearchOption.TopDirectoryOnly);
			DeleteAllDirectoriesWithSuffix(directory, suffix, SearchOption.TopDirectoryOnly);

			if(searchOption == SearchOption.AllDirectories) {
				DirectoryInfo[] allSubdirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);

				foreach(DirectoryInfo subdirectory in allSubdirectories) {
					DeleteAllWithSuffix(subdirectory, suffix, SearchOption.AllDirectories);
				}
			}
		}

		/// <summary>
		/// Deletes all directories in the specified directory, whose name is ended by the specified suffix, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to delete all directories.</param>
		/// <param name="suffix">The suffix that directories must contain in order to be deleted.</param>
		public static void DeleteAllDirectoriesWithSuffix(DirectoryInfo directory, string suffix)
		{
			DeleteAllDirectoriesWithSuffix(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Deletes all directories in the specified directory, whose name is ended by the specified suffix, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to delete all directories.</param>
		/// <param name="suffix">The suffix that directories must contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void DeleteAllDirectoriesWithSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			DirectoryInfo[] allSubdirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
			for(int i = allSubdirectories.Length - 1; i >= 0; i--) {
				DirectoryInfo subdirectory = allSubdirectories[i];

				if(subdirectory.Name.EndsWith(suffix)) {
					allSubdirectories[i].Delete(true);
					continue;
				}

				if(searchOption == SearchOption.AllDirectories) {
					DeleteAllDirectoriesWithSuffix(subdirectory, suffix, SearchOption.AllDirectories);
				}
			}
		}

		/// <summary>
		/// Deletes all files in the specified directory, whose name is ended by the specified suffix, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files.</param>
		/// <param name="suffix">The suffix that files must contain in order to be deleted.</param>
		public static void DeleteAllFilesWithSuffix(DirectoryInfo directory, string suffix)
		{
			DeleteAllFilesWithSuffix(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Deletes all files in the specified directory, whose name is ended by the specified suffix, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files.</param>
		/// <param name="suffix">The suffix that files must contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void DeleteAllFilesWithSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			FileInfo[] allFiles = directory.GetFiles("*" + suffix, searchOption);
			foreach(FileInfo file in allFiles) {
				file.Delete();
			}
		}

		/// <summary>
		/// Deletes all files and directories in the specified directory, whose name is not ended by the specified suffix, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files and directories.</param>
		/// <param name="suffix">The suffix that files and directories must not contain in order to be deleted.</param>
		public static void DeleteAllWithoutSuffix(DirectoryInfo directory, string suffix)
		{
			DeleteAllWithoutSuffix(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Deletes all files and directories in the specified directory, whose name is not ended by the specified suffix, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files and directories.</param>
		/// <param name="suffix">The suffix that files and directories must not contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void DeleteAllWithoutSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			DeleteAllWithoutSuffix(directory, suffix, searchOption, null);
		}

		/// <summary>
		/// Deletes all files and directories in the specified directory, whose name is not ended by the specified suffix, using the specified search option. If a directory matches any of the values in the provided ignore list, it will not be deleted.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files and directories.</param>
		/// <param name="suffix">The suffix that files and directories must not contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		/// <param name="ignoredDirectories">A collection of directory names to ignore.</param>
		public static void DeleteAllWithoutSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption,IEnumerable<string> ignoredDirectories)
		{
			DeleteAllFilesWithoutSuffix(directory, suffix, SearchOption.TopDirectoryOnly);
			DeleteAllDirectoriesWithoutSuffix(directory, suffix, SearchOption.TopDirectoryOnly,ignoredDirectories);

			if(searchOption == SearchOption.AllDirectories) {
				DirectoryInfo[] allSubdirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);

				foreach(DirectoryInfo subdirectory in allSubdirectories) {
					DeleteAllWithoutSuffix(subdirectory, suffix, SearchOption.AllDirectories,ignoredDirectories);
				}
			}
		}

		/// <summary>
		/// Deletes all directories in the specified directory, whose name is not ended by the specified suffix, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to delete all directories.</param>
		/// <param name="suffix">The suffix that directories must not contain in order to be deleted.</param>
		public static void DeleteAllDirectoriesWithoutSuffix(DirectoryInfo directory, string suffix)
		{
			DeleteAllDirectoriesWithoutSuffix(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Deletes all directories in the specified directory, whose name is not ended by the specified suffix, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to delete all directories.</param>
		/// <param name="suffix">The suffix that directories must not contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void DeleteAllDirectoriesWithoutSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			DeleteAllDirectoriesWithoutSuffix(directory, suffix, searchOption, null);
		}

		/// <summary>
		/// Deletes all directories in the specified directory, whose name is not ended by the specified suffix, using the specified search option. If a directory matches any of the values in the provided ignore list, it will not be deleted.
		/// </summary>
		/// <param name="directory">The directory in which to delete all directories.</param>
		/// <param name="suffix">The suffix that directories must not contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		/// <param name="ignoredDirectories">A collection of directory names to ignore.</param>
		public static void DeleteAllDirectoriesWithoutSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption,IEnumerable<string> ignoredDirectories)
		{
			DirectoryInfo[] allSubdirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
			for(int i = allSubdirectories.Length - 1; i >= 0; --i) {
				DirectoryInfo subdirectory = allSubdirectories[i];

				if(ignoredDirectories!=null && ignoredDirectories.Contains(subdirectory.Name)) {
					continue;
				}

				if(!subdirectory.Name.EndsWith(suffix)) {
					allSubdirectories[i].Delete(true);
					continue;
				}

				if(searchOption == SearchOption.AllDirectories) {
					DeleteAllDirectoriesWithoutSuffix(subdirectory, suffix, SearchOption.AllDirectories,ignoredDirectories);
				}
			}
		}

		/// <summary>
		/// Deletes all files in the specified directory, whose name is not ended by the specified suffix, including subdirectories.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files.</param>
		/// <param name="suffix">The suffix that files must not contain in order to be deleted.</param>
		public static void DeleteAllFilesWithoutSuffix(DirectoryInfo directory, string suffix)
		{
			DeleteAllFilesWithoutSuffix(directory, suffix, SearchOption.AllDirectories);
		}

		/// <summary>
		/// Deletes all files in the specified directory, whose name is not ended by the specified suffix, using the specified search option.
		/// </summary>
		/// <param name="directory">The directory in which to delete all files.</param>
		/// <param name="suffix">The suffix that files must not contain in order to be deleted.</param>
		/// <param name="searchOption">Specifies whether to include only the current directory or all subdirectories.</param>
		public static void DeleteAllFilesWithoutSuffix(DirectoryInfo directory, string suffix, SearchOption searchOption)
		{
			FileInfo[] allFiles = directory.GetFiles("*", searchOption);
			foreach(FileInfo file in allFiles) {
				if(!file.Name.EndsWith(suffix)) {
					file.Delete();
				}
			}
		}
	}
}
