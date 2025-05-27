using SteelSeriesCompanion.SharedCore;

namespace SteelSeriesCompanion.ExternalCommunication.Shared.Command
{
	public class SetChannelMuteCommand (SoundChannel channel, bool mute) : BaseExternalCommunicationCommand(COMMAND_NAME)
	{
		public SoundChannel channel = channel;
		public bool mute = mute;

		public const string COMMAND_NAME = "SetMute";

		public override void ExecuteCommand (object? sender, ISteelSeriesCompanionCore core)
		{
			base.ExecuteCommand(sender, core);
			core.SetChannelMute(sender, channel, mute);
		}
	}
}
