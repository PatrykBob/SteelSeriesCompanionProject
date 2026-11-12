using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SteelSeriesSonarCompanion.CoreApp.Extension
{
	public class SteelSeriesCompanionExtensionController : IDisposable
	{
		private List<BaseSteelSeriesCompanionExtension> ExtensionCollection { get; set; } = new();

		private const string EXTENSION_FOLDER_NAME = "Extension";
		private const string EXTENSION_POSTFIX = ".dll";

		public void Initialize (ISteelSeriesCompanionCore extensionCore)
		{
			SetupAssemblyResolve();
			LoadExtensions(extensionCore);
		}

		public List<SteelSeriesCompanionExtensionMenuItem> GetExtensionToolMenuItemCollection ()
		{
			List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection = new();

			for (int i = 0; i < ExtensionCollection.Count; i++)
			{
				extensionMenuItemCollection.Add(ExtensionCollection[i].GetExtensionMenuItem());
			}

			return extensionMenuItemCollection;
		}

		public void Dispose ()
		{
			for (int i = 0; i < ExtensionCollection.Count; i++)
			{
				try
				{
					ExtensionCollection[i].Dispose();
				}
				catch (Exception e)
				{
					Trace.WriteLine($"Failed to dispose extension '{ExtensionCollection[i].GetType().FullName}': {e}");
				}
			}

			ExtensionCollection.Clear();
		}

		private void SetupAssemblyResolve ()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				string assemblyName = new AssemblyName(args.Name).Name + EXTENSION_POSTFIX;
				string assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EXTENSION_FOLDER_NAME, assemblyName);

				if (File.Exists(assemblyPath))
				{
					return Assembly.LoadFrom(assemblyPath);
				}

				return null;
			};
		}

		private void LoadExtensions (ISteelSeriesCompanionCore extensionCore)
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EXTENSION_FOLDER_NAME);
			DirectoryInfo extensionDirectory = new(path);

			if (extensionDirectory.Exists == false)
			{
				extensionDirectory.Create();
			}

			FileInfo[] fileInfoCollection = extensionDirectory.GetFiles();

			for (int i = 0; i < fileInfoCollection.Length; i++)
			{
				LoadExtension(fileInfoCollection[i], extensionCore);
			}
		}

		private void LoadExtension (FileInfo extensionFile, ISteelSeriesCompanionCore extensionCore)
		{
			Type[] exportedTypeCollection;

			try
			{
				Assembly dll = Assembly.LoadFile(extensionFile.FullName);
				exportedTypeCollection = dll.GetExportedTypes();
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Skipping '{extensionFile.Name}': {e.Message}");
				return;
			}

			for (int i = 0; i < exportedTypeCollection.Length; i++)
			{
				Type exportedType = exportedTypeCollection[i];

				if (IsExtensionType(exportedType) == false)
				{
					continue;
				}

				try
				{
					object? instance = Activator.CreateInstance(exportedType);

					if (instance is BaseSteelSeriesCompanionExtension extension)
					{
						extension.Initialize(extensionCore);
						ExtensionCollection.Add(extension);
					}
				}
				catch (Exception e)
				{
					Trace.WriteLine($"Failed to initialize extension '{exportedType.FullName}': {e}");
				}
			}
		}

		private static bool IsExtensionType (Type type)
		{
			if (type == typeof(BaseSteelSeriesCompanionExtension))
			{
				return true;
			}

			if (type.BaseType != null)
			{
				return IsExtensionType(type.BaseType);
			}

			return false;
		}
	}
}
