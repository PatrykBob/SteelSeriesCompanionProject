using System.Diagnostics;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunication
{
	public class ServerIdentityStore
	{
		public string ServerId { get; private set; } = string.Empty;

		private const string FOLDER_NAME = "SteelSeriesSonarCompanion";
		private const string FILE_NAME = "serverId.txt";

		private static string FilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FOLDER_NAME, FILE_NAME);

		public void Load ()
		{
			try
			{
				if (File.Exists(FilePath) == true && Guid.TryParse(File.ReadAllText(FilePath).Trim(), out Guid storedId) == true)
				{
					ServerId = storedId.ToString();
					return;
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to load server id: {e.Message}");
			}

			ServerId = Guid.NewGuid().ToString();
			Save();
		}

		private void Save ()
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
				File.WriteAllText(FilePath, ServerId);
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to save server id: {e.Message}");
			}
		}
	}
}
