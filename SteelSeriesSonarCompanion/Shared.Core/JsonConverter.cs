using Newtonsoft.Json;

namespace SteelSeriesSonarCompanion.Shared.Core
{
	public static class JsonConverter
	{
		public static T? ConvertFromJSON<T> (string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static string ConvertToJSON<T> (T obj)
		{
			return JsonConvert.SerializeObject(obj);
		}
	}
}
