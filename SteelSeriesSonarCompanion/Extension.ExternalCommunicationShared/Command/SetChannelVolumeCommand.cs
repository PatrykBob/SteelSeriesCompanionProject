using SteelSeriesSonarCompanion.Shared.Core;
using SteelSeriesSonarCompanion.Shared.ExtensionCore.Extension;

namespace SteelSeriesSonarCompanion.Extension.ExternalCommunicationShared.Command
{
	public class SetChannelVolumeCommand (SoundChannel channel, float volume) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public float volume = volume;

		public const string COMMAND_NAME = "SetVolume";

		public override void ExecuteCommand (object? sender, ISteelSeriesCompanionCore core)
		{
			base.ExecuteCommand(sender, core);
			core.SetChannelVolume(sender, channel, volume);
		}
	}
}
