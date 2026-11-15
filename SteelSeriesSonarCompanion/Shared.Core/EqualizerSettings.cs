namespace SteelSeriesSonarCompanion.Shared.Core
{
	public class EqualizerSettings
	{
		public double Preamp { get; }
		public List<EqualizerFilter> Filters { get; }

		public EqualizerSettings (double preamp, List<EqualizerFilter> filters)
		{
			Preamp = preamp;
			Filters = filters;
		}
	}
}
