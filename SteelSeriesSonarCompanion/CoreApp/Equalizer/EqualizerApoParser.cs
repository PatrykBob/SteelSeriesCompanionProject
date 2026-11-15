using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.CoreApp.Equalizer
{
	public static partial class EqualizerApoParser
	{
		private const string ENABLED_STATE = "ON";
		private const double DEFAULT_Q_FACTOR = 0.7071;

		[GeneratedRegex(@"^Preamp\s*:\s*(?<gain>[-+]?[\d.]+)\s*dB", RegexOptions.IgnoreCase)]
		private static partial Regex PreampRegex ();

		[GeneratedRegex(@"^Filter\s*\d*\s*:\s*(?<state>ON|OFF)\s+(?<type>[A-Z]+)\b(?<parameters>.*)$", RegexOptions.IgnoreCase)]
		private static partial Regex FilterRegex ();

		[GeneratedRegex(@"\bFc\s+(?<value>[\d.]+)\s*Hz", RegexOptions.IgnoreCase)]
		private static partial Regex FrequencyRegex ();

		[GeneratedRegex(@"\bGain\s+(?<value>[-+]?[\d.]+)\s*dB", RegexOptions.IgnoreCase)]
		private static partial Regex GainRegex ();

		[GeneratedRegex(@"\bQ\s+(?<value>[\d.]+)", RegexOptions.IgnoreCase)]
		private static partial Regex QFactorRegex ();

		[GeneratedRegex(@"\bBW\s+Oct\s+(?<value>[\d.]+)", RegexOptions.IgnoreCase)]
		private static partial Regex BandwidthRegex ();

		public static bool TryParse (string text, out EqualizerSettings settings)
		{
			double preamp = 0.0;
			List<EqualizerFilter> filters = [];
			settings = new EqualizerSettings(preamp, filters);

			foreach (string rawLine in text.Split('\n'))
			{
				string line = rawLine.Trim().Replace(',', '.');
				Match preampMatch = PreampRegex().Match(line);

				if (preampMatch.Success == true)
				{
					if (TryParseNumber(preampMatch.Groups["gain"].Value, out preamp) == false)
					{
						return false;
					}

					continue;
				}

				Match filterMatch = FilterRegex().Match(line);

				if (filterMatch.Success == false)
				{
					continue;
				}

				if (TryParseFilter(filterMatch, out EqualizerFilter? filter) == false)
				{
					Trace.WriteLine($"Could not import equalizer line: {line}");
					return false;
				}

				filters.Add(filter!);
			}

			settings = new EqualizerSettings(preamp, filters);
			return filters.Count > 0;
		}

		private static bool TryParseFilter (Match match, out EqualizerFilter? filter)
		{
			filter = null;
			string parameters = match.Groups["parameters"].Value;

			if (TryConvertFilterType(match.Groups["type"].Value, out EqualizerFilterType type) == false)
			{
				return false;
			}

			Match frequencyMatch = FrequencyRegex().Match(parameters);

			if (frequencyMatch.Success == false || TryParseNumber(frequencyMatch.Groups["value"].Value, out double frequency) == false)
			{
				return false;
			}

			if (TryParseOptionalNumber(GainRegex().Match(parameters), 0.0, out double gain) == false || TryParseQFactor(parameters, out double qFactor) == false)
			{
				return false;
			}

			bool enabled = string.Equals(match.Groups["state"].Value, ENABLED_STATE, StringComparison.OrdinalIgnoreCase);
			filter = new EqualizerFilter(enabled, type, frequency, gain, qFactor);
			return true;
		}

		private static bool TryParseQFactor (string parameters, out double qFactor)
		{
			Match bandwidthMatch = BandwidthRegex().Match(parameters);

			if (bandwidthMatch.Success == false)
			{
				return TryParseOptionalNumber(QFactorRegex().Match(parameters), DEFAULT_Q_FACTOR, out qFactor);
			}

			qFactor = default;

			if (TryParseNumber(bandwidthMatch.Groups["value"].Value, out double octaves) == false || octaves <= 0.0)
			{
				return false;
			}

			double ratio = Math.Pow(2.0, octaves);
			qFactor = Math.Sqrt(ratio) / (ratio - 1.0);
			return true;
		}

		private static bool TryParseOptionalNumber (Match match, double defaultValue, out double value)
		{
			if (match.Success == false)
			{
				value = defaultValue;
				return true;
			}

			return TryParseNumber(match.Groups["value"].Value, out value);
		}

		private static bool TryParseNumber (string text, out double value)
		{
			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) == true && double.IsFinite(value) == true;
		}

		private static bool TryConvertFilterType (string apoType, out EqualizerFilterType type)
		{
			switch (apoType.ToUpperInvariant())
			{
				case "PK":
				case "PEQ":
					type = EqualizerFilterType.PEAKING;
					return true;
				case "LS":
				case "LSC":
					type = EqualizerFilterType.LOW_SHELF;
					return true;
				case "HS":
				case "HSC":
					type = EqualizerFilterType.HIGH_SHELF;
					return true;
				case "LP":
				case "LPQ":
					type = EqualizerFilterType.LOW_PASS;
					return true;
				case "HP":
				case "HPQ":
					type = EqualizerFilterType.HIGH_PASS;
					return true;
				case "NO":
					type = EqualizerFilterType.NOTCH;
					return true;
				default:
					type = default;
					return false;
			}
		}
	}
}
