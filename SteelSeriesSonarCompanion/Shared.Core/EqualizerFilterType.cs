using Newtonsoft.Json.Converters;

namespace SteelSeriesSonarCompanion.Shared.Core
{
	[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
	public enum EqualizerFilterType
	{
		PEAKING,
		LOW_SHELF,
		HIGH_SHELF,
		LOW_PASS,
		HIGH_PASS,
		NOTCH
	}
}
