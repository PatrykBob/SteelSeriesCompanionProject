namespace SteelSeriesSonarCompanion.Shared.Core
{
	public class EqualizerFilter
	{
		public bool Enabled { get; }
		public EqualizerFilterType Type { get; }
		public double Frequency { get; }
		public double Gain { get; }
		public double QFactor { get; }

		public EqualizerFilter (bool enabled, EqualizerFilterType type, double frequency, double gain, double qFactor)
		{
			Enabled = enabled;
			Type = type;
			Frequency = frequency;
			Gain = gain;
			QFactor = qFactor;
		}
	}
}
