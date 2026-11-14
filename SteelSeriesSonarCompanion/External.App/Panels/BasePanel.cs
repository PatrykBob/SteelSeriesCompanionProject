using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Connection;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public abstract class BasePanel : ContentView
	{
		public event Action<BasePanel> SettingsChanged = delegate { };

		public JObject Settings { get; private set; }
		public virtual bool HasSettings => false;

		protected CompanionConnection Connection { get; }

		protected BasePanel (CompanionConnection connection, JObject? settings)
		{
			Connection = connection;
			Settings = settings ?? new JObject();
		}

		public virtual Task<bool> TryEditSettings (Page hostPage)
		{
			return Task.FromResult(false);
		}

		public virtual void DetachFromEvents ()
		{
		}

		protected T GetSettings<T> () where T : new()
		{
			return Settings.ToObject<T>() ?? new T();
		}

		protected void SaveSettings<T> (T settings)
		{
			Settings = JObject.FromObject(settings!);
			SettingsChanged(this);
		}
	}
}
