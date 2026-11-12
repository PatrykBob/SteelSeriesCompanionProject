using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunication
{
	public class PairedDevice
	{
		public string Name { get; set; } = string.Empty;
		public string TokenHash { get; set; } = string.Empty;
		public DateTime PairedAt { get; set; }
	}

	public class PairedDeviceStore
	{
		private List<PairedDevice> DeviceCollection { get; set; } = new();

		private const string FOLDER_NAME = "SteelSeriesSonarCompanion";
		private const string FILE_NAME = "pairedDevices.json";
		private const int TOKEN_SIZE_BYTES = 32;

		private static string FilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FOLDER_NAME, FILE_NAME);

		public void Load ()
		{
			try
			{
				if (File.Exists(FilePath) == true)
				{
					DeviceCollection = JsonConverter.ConvertFromJSON<List<PairedDevice>>(File.ReadAllText(FilePath)) ?? new();
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to load paired devices: {e.Message}");
				DeviceCollection = new();
			}
		}

		public string Add (string deviceName)
		{
			string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TOKEN_SIZE_BYTES));

			DeviceCollection.Add(new PairedDevice
			{
				Name = deviceName,
				TokenHash = HashToken(token),
				PairedAt = DateTime.UtcNow
			});

			Save();
			return token;
		}

		public bool Contains (string? token)
		{
			if (string.IsNullOrEmpty(token) == true)
			{
				return false;
			}

			byte[] tokenHash = Encoding.ASCII.GetBytes(HashToken(token));
			bool isFound = false;

			for (int i = 0; i < DeviceCollection.Count; i++)
			{
				byte[] storedHash = Encoding.ASCII.GetBytes(DeviceCollection[i].TokenHash);

				if (CryptographicOperations.FixedTimeEquals(tokenHash, storedHash) == true)
				{
					isFound = true;
				}
			}

			return isFound;
		}

		public void Clear ()
		{
			DeviceCollection.Clear();
			Save();
		}

		private void Save ()
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
				File.WriteAllText(FilePath, JsonConverter.ConvertToJSON(DeviceCollection));
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to save paired devices: {e.Message}");
			}
		}

		private static string HashToken (string token)
		{
			return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
		}
	}
}
