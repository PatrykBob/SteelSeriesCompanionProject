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
			FileInfo[] fileInfoCollection = extensionDirectory.GetFiles();

			for (int i = 0; i < fileInfoCollection.Length; i++)
			{
				LoadExtension(fileInfoCollection[i]);
			}
		}

		private void LoadExtension (FileInfo extensionFile)
		{
			Assembly DLL = Assembly.LoadFile(extensionFile.FullName);

			foreach (Type type in DLL.GetExportedTypes())
			{
				object? instance = Activator.CreateInstance(type);

				if (instance is BaseSteelSeriesCompanionExtension extension)
				{
					ExtensionCollection.Add(extension);
				}
			}
		}
	}
}
