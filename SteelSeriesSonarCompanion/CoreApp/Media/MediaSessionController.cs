using System.Diagnostics;
using SteelSeriesSonarCompanion.Shared.Core;
using Windows.Foundation;
using Windows.Media.Control;

namespace SteelSeriesSonarCompanion.CoreApp.Media
{
	public class MediaSessionController : IDisposable
	{
		public event Action<MediaData> MediaChanged = delegate { };

		private GlobalSystemMediaTransportControlsSessionManager? SessionManager { get; set; }
		private GlobalSystemMediaTransportControlsSession? CurrentSession { get; set; }
		private object SessionLock { get; } = new();
		private int StateVersion { get; set; }

		public async Task Initialize ()
		{
			try
			{
				SessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to access media sessions: {e.Message}");
				return;
			}

			SessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
			AttachToCurrentSession();
		}

		public async Task<MediaData> GetMedia ()
		{
			GlobalSystemMediaTransportControlsSession? session = GetCurrentSession();

			if (session == null)
			{
				return MediaData.NoSession;
			}

			try
			{
				GlobalSystemMediaTransportControlsSessionMediaProperties properties = await session.TryGetMediaPropertiesAsync();
				bool isPlaying = session.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
				return new MediaData(true, isPlaying, properties?.Title ?? string.Empty, properties?.Artist ?? string.Empty);
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Failed to read the media session: {e.Message}");
				return MediaData.NoSession;
			}
		}

		public Task<bool> TryTogglePlayPause ()
		{
			return TryRunSessionAction(session => session.TryTogglePlayPauseAsync(), "toggle play/pause");
		}

		public Task<bool> TrySkipNext ()
		{
			return TryRunSessionAction(session => session.TrySkipNextAsync(), "skip to the next track");
		}

		public Task<bool> TrySkipPrevious ()
		{
			return TryRunSessionAction(session => session.TrySkipPreviousAsync(), "skip to the previous track");
		}

		private async Task<bool> TryRunSessionAction (Func<GlobalSystemMediaTransportControlsSession, IAsyncOperation<bool>> action, string actionName)
		{
			GlobalSystemMediaTransportControlsSession? session = GetCurrentSession();

			if (session == null)
			{
				Trace.WriteLine($"Could not {actionName}, no media session");
				return false;
			}

			try
			{
				return await action(session);
			}
			catch (Exception e)
			{
				Trace.WriteLine($"Could not {actionName}: {e.Message}");
				return false;
			}
		}

		private GlobalSystemMediaTransportControlsSession? GetCurrentSession ()
		{
			lock (SessionLock)
			{
				return CurrentSession;
			}
		}

		private void OnCurrentSessionChanged (GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
		{
			AttachToCurrentSession();
		}

		private void AttachToCurrentSession ()
		{
			lock (SessionLock)
			{
				DetachFromCurrentSession();
				CurrentSession = SessionManager?.GetCurrentSession();

				if (CurrentSession != null)
				{
					CurrentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
					CurrentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
				}
			}

			_ = PublishMedia();
		}

		private void DetachFromCurrentSession ()
		{
			if (CurrentSession == null)
			{
				return;
			}

			CurrentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
			CurrentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
			CurrentSession = null;
		}

		private void OnPlaybackInfoChanged (GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
		{
			_ = PublishMedia();
		}

		private void OnMediaPropertiesChanged (GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
		{
			_ = PublishMedia();
		}

		private async Task PublishMedia ()
		{
			int version;

			lock (SessionLock)
			{
				version = ++StateVersion;
			}

			MediaData media = await GetMedia();

			lock (SessionLock)
			{
				if (version != StateVersion)
				{
					return;
				}
			}

			MediaChanged(media);
		}

		public void Dispose ()
		{
			if (SessionManager != null)
			{
				SessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
			}

			lock (SessionLock)
			{
				DetachFromCurrentSession();
			}
		}
	}
}
