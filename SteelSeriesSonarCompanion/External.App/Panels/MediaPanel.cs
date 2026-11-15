using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Connection;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class MediaPanel : BasePanel
	{
		public const string TYPE_ID = "Media";
		public const string DISPLAY_NAME = "Media";

		private Label TitleLabel { get; } = new()
		{
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			HorizontalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		private Label ArtistLabel { get; } = new()
		{
			TextColor = Colors.Gray,
			HorizontalTextAlignment = TextAlignment.Center,
			LineBreakMode = LineBreakMode.TailTruncation
		};
		private Button PreviousButton { get; } = new()
		{
			Text = PREVIOUS_TEXT
		};
		private Button PlayPauseButton { get; } = new()
		{
			Text = PLAY_TEXT
		};
		private Button NextButton { get; } = new()
		{
			Text = NEXT_TEXT
		};

		private const string PREVIOUS_TEXT = "Previous";
		private const string PLAY_TEXT = "Play";
		private const string PAUSE_TEXT = "Pause";
		private const string NEXT_TEXT = "Next";
		private const string NOTHING_PLAYING_TEXT = "Nothing playing";
		private const string UNKNOWN_TITLE_TEXT = "Unknown title";

		public MediaPanel (CompanionConnection connection, JObject? settings) : base(connection, settings)
		{
			Content = CreateContent();
			PreviousButton.Clicked += OnPreviousClicked;
			PlayPauseButton.Clicked += OnPlayPauseClicked;
			NextButton.Clicked += OnNextClicked;
			Connection.MediaChanged += OnMediaChanged;
			ApplyMedia(Connection.TryGetMediaState(out MediaData? media) == true ? media! : MediaData.NoSession);
			Connection.RequestMedia();
		}

		public override void DetachFromEvents ()
		{
			Connection.MediaChanged -= OnMediaChanged;
		}

		private View CreateContent ()
		{
			HorizontalStackLayout buttonLayout = new()
			{
				Spacing = 12,
				HorizontalOptions = LayoutOptions.Center
			};
			buttonLayout.Add(PreviousButton);
			buttonLayout.Add(PlayPauseButton);
			buttonLayout.Add(NextButton);

			VerticalStackLayout rootLayout = new()
			{
				Padding = new Thickness(16, 8),
				Spacing = 8,
				VerticalOptions = LayoutOptions.Center
			};
			rootLayout.Add(TitleLabel);
			rootLayout.Add(ArtistLabel);
			rootLayout.Add(buttonLayout);
			return rootLayout;
		}

		private void OnMediaChanged (MediaData media)
		{
			ApplyMedia(media);
		}

		private void ApplyMedia (MediaData media)
		{
			if (media.HasSession == false)
			{
				TitleLabel.Text = NOTHING_PLAYING_TEXT;
				ArtistLabel.Text = string.Empty;
			}
			else
			{
				TitleLabel.Text = string.IsNullOrWhiteSpace(media.Title) == true ? UNKNOWN_TITLE_TEXT : media.Title;
				ArtistLabel.Text = media.Artist;
			}

			ArtistLabel.IsVisible = string.IsNullOrWhiteSpace(ArtistLabel.Text) == false;
			PlayPauseButton.Text = media.IsPlaying == true ? PAUSE_TEXT : PLAY_TEXT;
			PreviousButton.IsEnabled = media.HasSession;
			PlayPauseButton.IsEnabled = media.HasSession;
			NextButton.IsEnabled = media.HasSession;
		}

		private void OnPreviousClicked (object? sender, EventArgs e)
		{
			Connection.SkipMediaPrevious();
		}

		private void OnPlayPauseClicked (object? sender, EventArgs e)
		{
			Connection.ToggleMediaPlayPause();
		}

		private void OnNextClicked (object? sender, EventArgs e)
		{
			Connection.SkipMediaNext();
		}
	}
}
