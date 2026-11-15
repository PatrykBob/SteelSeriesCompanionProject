using Newtonsoft.Json;

namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ChannelConfigResponse
    {
        public string? id;
        public string? name;
        public string? createdAt;
        public string? updatedAt;
        public string? virtualAudioDevice;
        public GroupDataResponse? data;
        public int schemaVersion;
        public bool isPreset;
        public GroupDataResponse? defaultData;
        public string? image;
        public bool isFavorite;
        public int favoritePosition;
        public string? releaseVersion;

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class GroupDataResponse
        {
            public BaseDataResponse? bassBoostState;
            public BaseDataResponse? trebleBoostState;
            public BaseDataResponse? voiceClarityState;
            public SmartVolumeDataResponse? smartVolume;
            public double? generalGain;
            public EqualizerResponse? parametricEQ;
            public bool? virtualSurroundState;
            public GroupVirtualSurroundChannelResponse? virtualSurroundChannels;
            public double? reverbGainDB;
            public string? formFactor;
            public bool? globalEnableState;
            public bool? acousticEchoCancelingState;
            public BaseDataResponse? automaticNoiseGateState;
            public BaseDataResponse? impactNoiseReductionState;
            public BaseDataResponse? noiseCancelingState;
            public BaseDataResponse? noiseGateState;
            public BaseDataResponse? noiseReductionState;
            public BaseDataResponse? volumeStabilizerState;
        }

        public class BaseDataResponse
        {
            public bool enabled;
            public double value;
        }

        public class SmartVolumeDataResponse
        {
			public bool enabled;
			public double volumeLevel;
			public string? loudness;
        }

        public class EqualizerResponse
        {
            public bool enabled;
            public EqualizerFilterResponse? filter1;
            public EqualizerFilterResponse? filter2;
            public EqualizerFilterResponse? filter3;
            public EqualizerFilterResponse? filter4;
            public EqualizerFilterResponse? filter5;
            public EqualizerFilterResponse? filter6;
            public EqualizerFilterResponse? filter7;
            public EqualizerFilterResponse? filter8;
            public EqualizerFilterResponse? filter9;
            public EqualizerFilterResponse? filter10;
        }

        public class EqualizerFilterResponse
        {
            public bool enabled;
            public double qFactor;
            public double frequency;
            public double gain;
            public string? type;
        }

        public class GroupVirtualSurroundChannelResponse
        {
            public VirtualSurroundChannelResponse? frontLeft;
            public VirtualSurroundChannelResponse? frontRight;
            public VirtualSurroundChannelResponse? center;
            public VirtualSurroundChannelResponse? subWoofer;
            public VirtualSurroundChannelResponse? rearLeft;
            public VirtualSurroundChannelResponse? rearRight;
            public VirtualSurroundChannelResponse? sideLeft;
            public VirtualSurroundChannelResponse? sideRight;
        }

        public class VirtualSurroundChannelResponse
        {
            public double position;
            public double gain;
        }
    }
}
