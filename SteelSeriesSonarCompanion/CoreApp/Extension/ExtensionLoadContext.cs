using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace SteelSeriesSonarCompanion.CoreApp.Extension
{
	internal class ExtensionLoadContext : AssemblyLoadContext
	{
		private const string SHARED_ASSEMBLY_PREFIX = "SteelSeriesSonarCompanion.Shared.";
		private const string ASSEMBLY_POSTFIX = ".dll";

		private string ExtensionDirectory { get; set; }

		public ExtensionLoadContext (string extensionPath) : base(Path.GetFileNameWithoutExtension(extensionPath))
		{
			ExtensionDirectory = Path.GetDirectoryName(extensionPath)!;
		}

		protected override Assembly? Load (AssemblyName assemblyName)
		{
			if (assemblyName.Name == null || assemblyName.Name.StartsWith(SHARED_ASSEMBLY_PREFIX) == true)
			{
				return null;
			}

			string assemblyPath = Path.Combine(ExtensionDirectory, assemblyName.Name + ASSEMBLY_POSTFIX);

			if (File.Exists(assemblyPath) == true)
			{
				return LoadFromAssemblyPath(assemblyPath);
			}

			return null;
		}
	}
}
