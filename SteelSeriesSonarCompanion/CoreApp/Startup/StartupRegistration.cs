using Microsoft.Win32;
using System.Diagnostics;

namespace SteelSeriesSonarCompanion.CoreApp.Startup
{
	public class StartupRegistration
	{
		private const string RUN_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
		private const string VALUE_NAME = "SteelSeriesSonarCompanion";

		public bool IsEnabled ()
		{
			using RegistryKey? runKey = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH);
			return runKey?.GetValue(VALUE_NAME) is string value && value == GetCommand();
		}

		public bool TrySetEnabled (bool isEnabled)
		{
			try
			{
				using RegistryKey runKey = Registry.CurrentUser.CreateSubKey(RUN_KEY_PATH);

				if (isEnabled == true)
				{
					runKey.SetValue(VALUE_NAME, GetCommand());
				}
				else
				{
					runKey.DeleteValue(VALUE_NAME, false);
				}

				return true;
			}
			catch (Exception exception)
			{
				Trace.WriteLine($"Failed to update start with Windows: {exception}");
				return false;
			}
		}

		private string GetCommand ()
		{
			return $"\"{Environment.ProcessPath}\"";
		}
	}
}
