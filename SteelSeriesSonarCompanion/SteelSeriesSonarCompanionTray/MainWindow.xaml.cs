using SteelSeriesSonarCompanion.Communication.External;
using SteelSeriesSonarCompanion.Communication.Internal;
using System.Windows;

namespace SteelSeriesSonarCompanion
{
	public partial class MainWindow : Window
	{
		private InternalCommunicationFacade InternalFacade { get; } = new();
		private ExternalCommunicationFacade ExternalFacade { get; } = new();

		public MainWindow ()
		{
			Initialize();
			InitializeComponent();
		}

		~MainWindow ()
		{
			DetachFromEvents();
		}

		private async void Initialize ()
		{
			await InternalFacade.Initialize(6327);
			ExternalFacade.Initialize();
			AttachToEvents();
		}

		private void AttachToEvents ()
		{
			GameVolumeSlider.ValueChanged += UpdateGameVolume;
			ChatVolumeSlider.ValueChanged += UpdateChatVolume;
			MediaVolumeSlider.ValueChanged += UpdateMediaVolume;
			AuxVolumeSlider.ValueChanged += UpdateAuxVolume;
			ChatMixSlider.ValueChanged += UpdateChatMix;

			GameMuteCheckBox.Checked += UpdateGameMuteState;
			ChatMuteCheckBox.Checked += UpdateChatMuteState;
			MediaMuteCheckBox.Checked += UpdateMediaMuteState;
			AuxMuteCheckBox.Checked += UpdateAuxMuteState;
			GameMuteCheckBox.Unchecked += UpdateGameMuteState;
			ChatMuteCheckBox.Unchecked += UpdateChatMuteState;
			MediaMuteCheckBox.Unchecked += UpdateMediaMuteState;
			AuxMuteCheckBox.Unchecked += UpdateAuxMuteState;

			ExternalFacade.TestVolumeChanged += volume =>
			{
				Dispatcher.Invoke(() =>
				{
					GameVolumeSlider.Value = volume;
				});
			};
		}

		private void DetachFromEvents ()
		{
			GameVolumeSlider.ValueChanged -= UpdateGameVolume;
			ChatVolumeSlider.ValueChanged -= UpdateChatVolume;
			MediaVolumeSlider.ValueChanged -= UpdateMediaVolume;
			AuxVolumeSlider.ValueChanged -= UpdateAuxVolume;

			GameMuteCheckBox.Checked -= UpdateGameMuteState;
			ChatMuteCheckBox.Checked -= UpdateChatMuteState;
			MediaMuteCheckBox.Checked -= UpdateMediaMuteState;
			AuxMuteCheckBox.Checked -= UpdateAuxMuteState;
			GameMuteCheckBox.Unchecked -= UpdateGameMuteState;
			ChatMuteCheckBox.Unchecked -= UpdateChatMuteState;
			MediaMuteCheckBox.Unchecked -= UpdateMediaMuteState;
			AuxMuteCheckBox.Unchecked -= UpdateAuxMuteState;
		}

		private void UpdateGameVolume (object sender, RoutedPropertyChangedEventArgs<double> volume)
		{
			SetChannelVolume("game", volume.NewValue);
		}

		private void UpdateChatVolume (object sender, RoutedPropertyChangedEventArgs<double> volume)
		{
			SetChannelVolume("chatRender", volume.NewValue);
		}

		private void UpdateMediaVolume (object sender, RoutedPropertyChangedEventArgs<double> volume)
		{
			SetChannelVolume("media", volume.NewValue);
		}

		private void UpdateAuxVolume (object sender, RoutedPropertyChangedEventArgs<double> volume)
		{
			SetChannelVolume("aux", volume.NewValue);
		}

		private void UpdateChatMix (object sender, RoutedPropertyChangedEventArgs<double> mix)
		{
			SetChatMix(mix.NewValue);
		}

		private void UpdateGameMuteState (object sender, RoutedEventArgs e)
		{
			SetChannelMute("game", GameMuteCheckBox.IsChecked == true);
		}

		private void UpdateChatMuteState (object sender, RoutedEventArgs e)
		{
			SetChannelMute("chatRender", ChatMuteCheckBox.IsChecked == true);
		}

		private void UpdateMediaMuteState (object sender, RoutedEventArgs e)
		{
			SetChannelMute("media", MediaMuteCheckBox.IsChecked == true);
		}

		private void UpdateAuxMuteState (object sender, RoutedEventArgs e)
		{
			SetChannelMute("aux", AuxMuteCheckBox.IsChecked == true);
		}

		private async void SetChannelVolume (string channel, double volume)
		{
			await InternalFacade.SetChannelVolume(channel, (float)volume);
		}

		private async void SetChatMix (double mix)
		{
			await InternalFacade.SetChatMix((float)mix);
		}

		private async void SetChannelMute (string channel, bool mute)
		{
			await InternalFacade.SetChannelMute(channel, mute);
		}
	}
}