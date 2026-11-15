using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public static class ExternalCommunicationCommandConverter
	{
		public static string CovertToJson (BaseExternalCommunicationCommand command)
		{
			return JsonConverter.ConvertToJSON(command);
		}

		public static BaseExternalCommunicationCommand? ConvertFromJson (string json)
		{
			BaseExternalCommunicationCommand? command = JsonConverter.ConvertFromJSON<BaseExternalCommunicationCommand>(json);

			if (command != null)
			{
				return command.CommandName switch
				{
					SetChannelMuteCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SetChannelMuteCommand>(json),
					SetChannelVolumeCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SetChannelVolumeCommand>(json),
					RequestVolumeSetupCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<RequestVolumeSetupCommand>(json),
					RefreshAudioDeviceCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<RefreshAudioDeviceCommand>(json),
					RequestEqualizerCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<RequestEqualizerCommand>(json),
					RequestEqualizerPresetsCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<RequestEqualizerPresetsCommand>(json),
					SetEqualizerEnabledCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SetEqualizerEnabledCommand>(json),
					SelectEqualizerPresetCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SelectEqualizerPresetCommand>(json),
					SaveEqualizerPresetCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SaveEqualizerPresetCommand>(json),
					ImportEqualizerPresetCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<ImportEqualizerPresetCommand>(json),
					RequestMediaCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<RequestMediaCommand>(json),
					ToggleMediaPlayPauseCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<ToggleMediaPlayPauseCommand>(json),
					SkipMediaNextCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SkipMediaNextCommand>(json),
					SkipMediaPreviousCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<SkipMediaPreviousCommand>(json),
					AuthenticateCommand.COMMAND_NAME => JsonConverter.ConvertFromJSON<AuthenticateCommand>(json),
					_ => null,
				};
			}

			return null;
		}
	}
}
