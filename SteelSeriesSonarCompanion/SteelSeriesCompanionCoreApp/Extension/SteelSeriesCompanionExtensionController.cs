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

		public List<ToolStripMenuItem> GetExtensionToolMenuItemCollection ()
		{
			List<ToolStripMenuItem> extensionMenuItemCollection = new();

			for (int i = 0; i < ExtensionCollection.Count; i++)
			{
				extensionMenuItemCollection.Add(ExtensionCollection[i].GetExtensionMenuItem());
			}

			return extensionMenuItemCollection;
		}

		private void LoadExtension (FileInfo extensionFile, ISteelSeriesCompanionCore extensionCore)
		{
			Assembly DLL = Assembly.LoadFile(extensionFile.FullName);

			foreach (Type type in DLL.GetExportedTypes())
			{
				object? instance = Activator.CreateInstance(type);

				if (instance is BaseSteelSeriesCompanionExtension extension)
				{
					extension.Initialize(extensionCore);
					ExtensionCollection.Add(extension);
				}
			}
		}
	}
}
