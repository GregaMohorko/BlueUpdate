using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlueUpdate.Utility
{
	public static class ReflectionUtility
	{
		/// <summary>
		/// Specifies the type of the assembly.
		/// </summary>
		public enum AssemblyType
		{
			Unknown = 0,
			/// <summary>
			/// Represents the entry assembly, which is usually the assembly of the application.
			/// </summary>
			Application = 1,
			/// <summary>
			/// Represents the assembly of the library where this enum is defined.
			/// </summary>
			Library = 2,
			/// <summary>
			/// Represents the currently running assembly.
			/// </summary>
			Current = 3
		}

		/// <summary>
		/// Structure with assembly information.
		/// </summary>
		public struct AssemblyInformation
		{
			public readonly string Title;
			public readonly string Description;
			public readonly string Company;
			public readonly string Product;
			public readonly string Copyright;
			public readonly string Trademark;
			public readonly Version Version;

			public AssemblyInformation(string title, string description, string company, string product, string copyright, string trademark, Version version)
			{
				Title = title;
				Description = description;
				Company = company;
				Product = product;
				Copyright = copyright;
				Trademark = trademark;
				Version = version;
			}
		}

		/// <summary>
		/// Gets the assembly of the specified type.
		/// </summary>
		/// <param name="assemblyType">The type of the assembly to look for.</param>
		public static Assembly GetAssembly(AssemblyType assemblyType)
		{
			Assembly assembly;

			switch(assemblyType) {
				case AssemblyType.Application:
					assembly = Assembly.GetEntryAssembly();
					break;
				case AssemblyType.Library:
					assembly = Assembly.GetExecutingAssembly();
					break;
				case AssemblyType.Current:
					assembly = null;
					break;
				default:
					throw new ArgumentException($"Unsupported assembly type '{assemblyType}'.");
			}

			if(assembly == null) {
				assembly = Assembly.GetCallingAssembly();
			}
			if(assembly == null) {
				throw new Exception("Assembly information could not be found.");
			}

			return assembly;
		}

		/// <summary>
		/// Gets the assembly information of the specified type.
		/// </summary>
		/// <param name="assemblyType">The type of the assembly to look for.</param>
		public static AssemblyInformation GetAssemblyInformation(AssemblyType assemblyType)
		{
			var assembly = GetAssembly(assemblyType);
			return GetAssemblyInformation(assembly);
		}

		/// <summary>
		/// Gets the assembly information of the specified type.
		/// </summary>
		/// <param name="assembly">The assembly from which to extract the information from.</param>
		public static AssemblyInformation GetAssemblyInformation(Assembly assembly)
		{
			string title = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			string description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
			string company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
			string product = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
			string copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
			string trademark = assembly.GetCustomAttribute<AssemblyTrademarkAttribute>().Trademark;

			string versionS;
			AssemblyVersionAttribute assemblyVersion = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
			if(assemblyVersion != null) {
				versionS = assemblyVersion.Version;
			} else {
				AssemblyFileVersionAttribute fileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
				if(fileVersion == null) {
					throw new Exception("Assembly version could not be found.");
				}
				versionS = fileVersion.Version;
			}
			Version version = Version.Parse(versionS);

			return new AssemblyInformation(title, description, company, product, copyright, trademark, version);
		}
	}
}
