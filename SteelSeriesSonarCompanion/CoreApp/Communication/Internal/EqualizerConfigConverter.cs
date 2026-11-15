using System.Diagnostics;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
	public static class EqualizerConfigConverter
	{
		private const string PEAKING_TYPE = "peakingEQ";
		private const string LOW_SHELF_TYPE = "lowShelving";
		private const string HIGH_SHELF_TYPE = "highShelving";
		private const string LOW_PASS_TYPE = "lowPass";
		private const string HIGH_PASS_TYPE = "highPass";
		private const string NOTCH_TYPE = "notchFilter";

		public static EqualizerData? ConvertToEqualizerData (SoundChannel channel, ChannelConfigResponse config)
		{
			ChannelConfigResponse.EqualizerResponse? equalizer = config.data?.parametricEQ;

			if (equalizer == null)
			{
				Trace.WriteLine($"Sonar returned no equalizer data for {channel}");
				return null;
			}

			List<EqualizerFilter> filters = GetFilterSlots(equalizer)
				.Where(filter => filter != null)
				.Select(filter => ConvertToEqualizerFilter(filter!))
				.ToList();

			EqualizerSettings settings = new(config.data!.generalGain ?? 0.0, filters);
			return new EqualizerData(channel, equalizer.enabled, ConvertToEqualizerPreset(config), settings);
		}

		public static EqualizerPreset ConvertToEqualizerPreset (ChannelConfigResponse config)
		{
			return new EqualizerPreset(config.id ?? string.Empty, config.name ?? string.Empty, config.isPreset);
		}

		private static EqualizerFilter ConvertToEqualizerFilter (ChannelConfigResponse.EqualizerFilterResponse filter)
		{
			return new EqualizerFilter(filter.enabled, ConvertToFilterType(filter.type), filter.frequency, filter.gain, filter.qFactor);
		}

		private static ChannelConfigResponse.EqualizerFilterResponse?[] GetFilterSlots (ChannelConfigResponse.EqualizerResponse equalizer)
		{
			return
			[
				equalizer.filter1, equalizer.filter2, equalizer.filter3, equalizer.filter4, equalizer.filter5,
				equalizer.filter6, equalizer.filter7, equalizer.filter8, equalizer.filter9, equalizer.filter10
			];
		}

		private static EqualizerFilterType ConvertToFilterType (string? type)
		{
			switch (type)
			{
				case PEAKING_TYPE:
					return EqualizerFilterType.PEAKING;
				case LOW_SHELF_TYPE:
					return EqualizerFilterType.LOW_SHELF;
				case HIGH_SHELF_TYPE:
					return EqualizerFilterType.HIGH_SHELF;
				case LOW_PASS_TYPE:
					return EqualizerFilterType.LOW_PASS;
				case HIGH_PASS_TYPE:
					return EqualizerFilterType.HIGH_PASS;
				case NOTCH_TYPE:
					return EqualizerFilterType.NOTCH;
				default:
					Trace.WriteLine($"Unknown Sonar equalizer filter type {type}, treated as peaking");
					return EqualizerFilterType.PEAKING;
			}
		}
	}
}
