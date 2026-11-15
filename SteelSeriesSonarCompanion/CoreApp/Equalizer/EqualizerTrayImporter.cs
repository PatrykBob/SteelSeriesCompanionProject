using System.Diagnostics;
using System.IO;
using SteelSeriesSonarCompanion.Shared.Core;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.CoreApp.Equalizer
{
	public class EqualizerTrayImporter
	{
		private ISteelSeriesCompanionCore Core { get; }

		private const string MENU_TEXT = "Import Equalizer Preset";
		private const string FILE_DIALOG_TITLE = "Choose an AutoEQ ParametricEQ.txt file for {0}";
		private const string FILE_DIALOG_FILTER = "AutoEQ ParametricEQ (*.txt)|*.txt|All files (*.*)|*.*";
		private const string QUESTION_TITLE = "Import Equalizer Preset";
		private const string NEW_OR_OVERWRITE_QUESTION_FORMAT = "Import \"{0}\" into {1}?\n\nYes - save as a new preset \"{0}\"\nNo - overwrite the current preset \"{2}\"";
		private const string NEW_ONLY_QUESTION_FORMAT = "Import \"{0}\" into {1} as a new preset?\n\nThe current preset \"{2}\" is a SteelSeries preset and cannot be overwritten.";
		private const string IMPORTED_MESSAGE_FORMAT = "Imported \"{0}\" into {1}.";
		private const string NO_EQUALIZER_MESSAGE_FORMAT = "Could not read the {0} equalizer from Sonar.";
		private const string FILE_READ_FAILED_MESSAGE = "The equalizer file could not be read.";
		private const string FILE_TOO_LARGE_MESSAGE = "The file is too large for an equalizer preset.";
		private const string AUTOEQ_FILE_SUFFIX = " ParametricEQ";
		private const int MAX_FILE_LENGTH = 65536;

		public EqualizerTrayImporter (ISteelSeriesCompanionCore core)
		{
			Core = core;
		}

		public SteelSeriesCompanionExtensionMenuItem CreateMenuItem ()
		{
			SteelSeriesCompanionExtensionMenuItem menuItem = new(MENU_TEXT);

			foreach (SoundChannel channel in Enum.GetValues<SoundChannel>().Where(channel => channel != SoundChannel.MASTER))
			{
				menuItem.AddSubMenuItem(new SteelSeriesCompanionExtensionMenuItem(channel.ToString(), () => _ = Import(channel)));
			}

			return menuItem;
		}

		private async Task Import (SoundChannel channel)
		{
			string? filePath = PickFile(channel);

			if (filePath == null || TryReadFile(filePath, out string text) == false)
			{
				return;
			}

			EqualizerData? equalizer = await Core.GetEqualizer(channel);

			if (equalizer == null)
			{
				Core.ShowNotification(string.Format(NO_EQUALIZER_MESSAGE_FORMAT, channel));
				return;
			}

			string newPresetName = CreatePresetName(filePath);

			if (TryAskForTarget(channel, equalizer.Preset, newPresetName, out bool isNewPreset) == false)
			{
				return;
			}

			if (await Core.TryImportEqualizerPreset(this, channel, text, isNewPreset == true ? newPresetName : null) == true)
			{
				Core.ShowNotification(string.Format(IMPORTED_MESSAGE_FORMAT, isNewPreset == true ? newPresetName : equalizer.Preset.Name, channel));
			}
		}

		private string? PickFile (SoundChannel channel)
		{
			using OpenFileDialog dialog = new()
			{
				Title = string.Format(FILE_DIALOG_TITLE, channel),
				Filter = FILE_DIALOG_FILTER
			};

			return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
		}

		private bool TryReadFile (string filePath, out string text)
		{
			text = string.Empty;

			try
			{
				if (new FileInfo(filePath).Length > MAX_FILE_LENGTH)
				{
					Core.ShowNotification(FILE_TOO_LARGE_MESSAGE);
					return false;
				}

				text = File.ReadAllText(filePath);
				return true;
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to read equalizer file {filePath}: {e.Message}");
				Core.ShowNotification(FILE_READ_FAILED_MESSAGE);
				return false;
			}
		}

		private bool TryAskForTarget (SoundChannel channel, EqualizerPreset currentPreset, string newPresetName, out bool isNewPreset)
		{
			isNewPreset = true;

			if (currentPreset.IsFactoryPreset == true)
			{
				string question = string.Format(NEW_ONLY_QUESTION_FORMAT, newPresetName, channel, currentPreset.Name);
				return MessageBox.Show(question, QUESTION_TITLE, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
			}

			string newOrOverwriteQuestion = string.Format(NEW_OR_OVERWRITE_QUESTION_FORMAT, newPresetName, channel, currentPreset.Name);
			DialogResult result = MessageBox.Show(newOrOverwriteQuestion, QUESTION_TITLE, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			isNewPreset = result == DialogResult.Yes;
			return result != DialogResult.Cancel;
		}

		private string CreatePresetName (string filePath)
		{
			string name = Path.GetFileNameWithoutExtension(filePath);
			return name.EndsWith(AUTOEQ_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase) == true ? name[..^AUTOEQ_FILE_SUFFIX.Length] : name;
		}
	}
}
