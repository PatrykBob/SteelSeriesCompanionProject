using Newtonsoft.Json;

namespace SteelSeriesCompanion.SharedCore.Converters
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
