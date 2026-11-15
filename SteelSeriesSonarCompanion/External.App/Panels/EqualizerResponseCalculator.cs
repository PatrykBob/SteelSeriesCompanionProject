using System.Numerics;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public static class EqualizerResponseCalculator
	{
		private const double SAMPLE_RATE = 48000.0;

		public static double GetGainDb (EqualizerSettings settings, double frequency)
		{
			double gainDb = settings.Preamp;

			foreach (EqualizerFilter filter in settings.Filters)
			{
				if (filter.Enabled == true)
				{
					gainDb += GetFilterGainDb(filter, frequency);
				}
			}

			return gainDb;
		}

		public static double GetFilterGainDb (EqualizerFilter filter, double frequency)
		{
			(double b0, double b1, double b2, double a0, double a1, double a2) = GetCoefficients(filter);
			Complex z1 = Complex.FromPolarCoordinates(1.0, -2.0 * Math.PI * frequency / SAMPLE_RATE);
			Complex z2 = z1 * z1;
			double magnitude = Complex.Abs((b0 + b1 * z1 + b2 * z2) / (a0 + a1 * z1 + a2 * z2));
			return 20.0 * Math.Log10(Math.Max(magnitude, 1e-6));
		}

		private static (double b0, double b1, double b2, double a0, double a1, double a2) GetCoefficients (EqualizerFilter filter)
		{
			double w0 = 2.0 * Math.PI * Math.Clamp(filter.Frequency, 1.0, SAMPLE_RATE / 2.0 - 1.0) / SAMPLE_RATE;
			double cos = Math.Cos(w0);
			double alpha = Math.Sin(w0) / (2.0 * Math.Max(filter.QFactor, 0.01));
			double a = Math.Pow(10.0, filter.Gain / 40.0);
			double shelfAlpha = 2.0 * Math.Sqrt(a) * alpha;

			return filter.Type switch
			{
				EqualizerFilterType.LOW_SHELF => (
					a * ((a + 1) - (a - 1) * cos + shelfAlpha),
					2 * a * ((a - 1) - (a + 1) * cos),
					a * ((a + 1) - (a - 1) * cos - shelfAlpha),
					(a + 1) + (a - 1) * cos + shelfAlpha,
					-2 * ((a - 1) + (a + 1) * cos),
					(a + 1) + (a - 1) * cos - shelfAlpha),
				EqualizerFilterType.HIGH_SHELF => (
					a * ((a + 1) + (a - 1) * cos + shelfAlpha),
					-2 * a * ((a - 1) + (a + 1) * cos),
					a * ((a + 1) + (a - 1) * cos - shelfAlpha),
					(a + 1) - (a - 1) * cos + shelfAlpha,
					2 * ((a - 1) - (a + 1) * cos),
					(a + 1) - (a - 1) * cos - shelfAlpha),
				EqualizerFilterType.LOW_PASS => ((1 - cos) / 2, 1 - cos, (1 - cos) / 2, 1 + alpha, -2 * cos, 1 - alpha),
				EqualizerFilterType.HIGH_PASS => ((1 + cos) / 2, -(1 + cos), (1 + cos) / 2, 1 + alpha, -2 * cos, 1 - alpha),
				EqualizerFilterType.NOTCH => (1, -2 * cos, 1, 1 + alpha, -2 * cos, 1 - alpha),
				_ => (1 + alpha * a, -2 * cos, 1 - alpha * a, 1 + alpha / a, -2 * cos, 1 - alpha / a),
			};
		}
	}
}
