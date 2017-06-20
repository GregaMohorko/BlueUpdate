using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlueUpdate.Security.Cryptography
{
	/// <summary>
	/// Provides utility functions for calculations using the SHA-256 algorithm.
	/// </summary>
	public static class Sha256
	{
		/// <summary>
		/// Calculates the checksum for the specified file.
		/// </summary>
		/// <param name="file">The path of the file for which to calculate the checksum.</param>
		public static string GetChecksum(string file)
		{
			var sha256 = new SHA256Managed();
			byte[] checksumBytes;

			using(FileStream fileStream = File.OpenRead(file)) {
				checksumBytes = sha256.ComputeHash(fileStream);
			}

			// remove the hyphen
			return BitConverter.ToString(checksumBytes).Replace("-", string.Empty);
		}
	}
}
