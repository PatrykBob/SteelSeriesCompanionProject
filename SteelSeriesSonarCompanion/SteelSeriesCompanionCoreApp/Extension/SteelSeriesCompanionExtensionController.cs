using SteelSeriesCompanion.SharedCore;
using System.IO;
using System.Reflection;

namespace SteelSeriesCompanion.Extension
{
	public class SteelSeriesCompanionExtensionController
	{
		private List<BaseSteelSeriesCompanionExtension> ExtensionCollection { get; set; } = new();

		private const string EXTENSION_FOLDER_NAME = "Extension";

		public void Initialize (ISteelSeriesCompanionCore extensionCore)
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

		public List<SteelSeriesCompanionExtensionMenuItem> GetExtensionToolMenuItemCollection ()
		{
			List<SteelSeriesCompanionExtensionMenuItem> extensionMenuItemCollection = new();

			for (int i = 0; i < ExtensionCollection.Count; i++)
			{
				extensionMenuItemCollection.Add(ExtensionCollection[i].GetExtensionMenuItem());
			}

			return extensionMenuItemCollection;
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

			foreach (Type type in dll.GetExportedTypes())
			{
				if (IsCorrectType(type) == true)
				{
					object? instance = Activator.CreateInstance(type);

					if (instance is BaseSteelSeriesCompanionExtension extension)
					{
						extension.Initialize(extensionCore);
						ExtensionCollection.Add(extension);
					}
				}
			}

			bool IsCorrectType (Type type)
			{
				if (type == typeof(BaseSteelSeriesCompanionExtension))
				{
					return true;
				}

				if (type.BaseType != null)
				{
					return IsCorrectType(type.BaseType);
				}

				return false;
			}
		}
	}
}
