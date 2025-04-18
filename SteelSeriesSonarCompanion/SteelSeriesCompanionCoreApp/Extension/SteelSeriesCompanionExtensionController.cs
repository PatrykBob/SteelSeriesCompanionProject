using SteelSeriesCompanion.SharedCore;
using System.IO;
using System.Reflection;

namespace SteelSeriesCompanion.Extension
{
	public class SteelSeriesCompanionExtensionController
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

		private void SetupAssemblyResolve ()
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
			{
				string assemblyName = args.Name + EXTENSION_POSTFIX;
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
			Assembly dll;

			try
			{
				dll = Assembly.LoadFile(extensionFile.FullName);
			}
			catch (Exception e)
			{
				return;
			}

			Type[] exportedTypeCollection = dll.GetExportedTypes();

			for (int i = 0; i < exportedTypeCollection.Length; i++)
			{
				Type exportedType = exportedTypeCollection[i];

				if (IsExtensionType(exportedType) == true)
				{
					object? instance = Activator.CreateInstance(exportedType);

					if (instance is BaseSteelSeriesCompanionExtension extension)
					{
						extension.Initialize(extensionCore);
						ExtensionCollection.Add(extension);
					}
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
