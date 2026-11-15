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
		private const double DEFAULT_FREQUENCY = 1000.0;
		private const double DEFAULT_Q_FACTOR = 0.7071;

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

		public static bool TryApplySettings (ChannelConfigResponse config, EqualizerSettings settings)
		{
			ChannelConfigResponse.GroupDataResponse? data = config.data;
			ChannelConfigResponse.EqualizerResponse? equalizer = data?.parametricEQ;

			if (data == null || equalizer == null || settings.Filters.Count > EqualizerLimits.MAX_FILTER_COUNT)
			{
				return false;
			}

			if (data.generalGain != null)
			{
				data.generalGain = Math.Clamp(settings.Preamp, EqualizerLimits.MIN_PREAMP, EqualizerLimits.MAX_PREAMP);
			}
			else if (settings.Preamp != 0.0)
			{
				Trace.WriteLine($"Sonar config {config.virtualAudioDevice} has no gain, preamp {settings.Preamp} ignored");
			}

			ChannelConfigResponse.EqualizerFilterResponse?[] currentSlots = GetFilterSlots(equalizer);
			ChannelConfigResponse.EqualizerFilterResponse[] newSlots = new ChannelConfigResponse.EqualizerFilterResponse[EqualizerLimits.MAX_FILTER_COUNT];

			for (int i = 0; i < newSlots.Length; i++)
			{
				newSlots[i] = i < settings.Filters.Count ? ConvertToFilterResponse(settings.Filters[i]) : CreateDisabledFilter(currentSlots[i]);
			}

			SetFilterSlots(equalizer, newSlots);
			return true;
		}

		private static EqualizerFilter ConvertToEqualizerFilter (ChannelConfigResponse.EqualizerFilterResponse filter)
		{
			return new EqualizerFilter(filter.enabled, ConvertToFilterType(filter.type), filter.frequency, filter.gain, filter.qFactor);
		}

		private static ChannelConfigResponse.EqualizerFilterResponse ConvertToFilterResponse (EqualizerFilter filter)
		{
			return new ChannelConfigResponse.EqualizerFilterResponse
			{
				enabled = filter.Enabled,
				type = ConvertToSonarFilterType(filter.Type),
				frequency = Math.Clamp(filter.Frequency, EqualizerLimits.MIN_FREQUENCY, EqualizerLimits.MAX_FREQUENCY),
				gain = Math.Clamp(filter.Gain, EqualizerLimits.MIN_GAIN, EqualizerLimits.MAX_GAIN),
				qFactor = Math.Clamp(filter.QFactor, EqualizerLimits.MIN_Q_FACTOR, EqualizerLimits.MAX_Q_FACTOR),
			};
		}

		private static ChannelConfigResponse.EqualizerFilterResponse CreateDisabledFilter (ChannelConfigResponse.EqualizerFilterResponse? currentFilter)
		{
			return new ChannelConfigResponse.EqualizerFilterResponse
			{
				enabled = false,
				type = currentFilter?.type ?? PEAKING_TYPE,
				frequency = currentFilter?.frequency ?? DEFAULT_FREQUENCY,
				gain = currentFilter?.gain ?? 0.0,
				qFactor = currentFilter?.qFactor ?? DEFAULT_Q_FACTOR,
			};
		}

		private static ChannelConfigResponse.EqualizerFilterResponse?[] GetFilterSlots (ChannelConfigResponse.EqualizerResponse equalizer)
		{
			return
			[
				equalizer.filter1, equalizer.filter2, equalizer.filter3, equalizer.filter4, equalizer.filter5,
				equalizer.filter6, equalizer.filter7, equalizer.filter8, equalizer.filter9, equalizer.filter10
			];
		}

		private static void SetFilterSlots (ChannelConfigResponse.EqualizerResponse equalizer, ChannelConfigResponse.EqualizerFilterResponse[] slots)
		{
			equalizer.filter1 = slots[0];
			equalizer.filter2 = slots[1];
			equalizer.filter3 = slots[2];
			equalizer.filter4 = slots[3];
			equalizer.filter5 = slots[4];
			equalizer.filter6 = slots[5];
			equalizer.filter7 = slots[6];
			equalizer.filter8 = slots[7];
			equalizer.filter9 = slots[8];
			equalizer.filter10 = slots[9];
		}

		private static string ConvertToSonarFilterType (EqualizerFilterType type)
		{
			return type switch
			{
				EqualizerFilterType.LOW_SHELF => LOW_SHELF_TYPE,
				EqualizerFilterType.HIGH_SHELF => HIGH_SHELF_TYPE,
				EqualizerFilterType.LOW_PASS => LOW_PASS_TYPE,
				EqualizerFilterType.HIGH_PASS => HIGH_PASS_TYPE,
				EqualizerFilterType.NOTCH => NOTCH_TYPE,
				_ => PEAKING_TYPE,
			};
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
