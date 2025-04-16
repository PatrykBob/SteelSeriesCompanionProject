using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Command
{
	public class SetChannelVolumeCommand (SoundChannel channel, float volume) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public float volume = volume;

		public const string COMMAND_NAME = "SetVolume";

		public override void ExecuteCommand (ISteelSeriesCompanionCore core)
		{
			core.SetChannelVolume(channel, volume);
		}

		public class VolumeData (SoundChannel channel, float volume)
		{
			public SoundChannel channel = channel;
			public float volume = volume;
		}
	}
}
