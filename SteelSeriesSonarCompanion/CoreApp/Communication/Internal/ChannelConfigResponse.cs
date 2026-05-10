namespace SteelSeriesSonarCompanion.CoreApp.Communication.Internal
{
    public class ChannelConfigResponse
    {
        public string id;
        public string name;
        public string createdAt;
        public string updatedAt;
        public string virtualAudioDevice;
        public GroupDataResponse data;
        public int schemaVersion;
        public bool isPreset;
        public GroupDataResponse defaultData;
        public string image;
        public bool isFavorite;
        public int favoritePosition;
        public string releaseVersion;

        public class GroupDataResponse
        {
            public BaseDataResponse bassBoostState;
            public BaseDataResponse trebleBoostState;
            public BaseDataResponse voiceClarityState;
            public SmartVolumeDataResponse smartVolume;
            public float generalGain;
            public EqualizerResponse parametricEQ;
            public bool virtualSurroundState;
            public GroupVirtualSurroundChannelResponse virtualSurroundChannels;
            public float reverbGainDB;
            public string formFactor;
            public bool globalEnableState;
        }

        public class BaseDataResponse
        {
            public bool enabled;
            public float value;
        }

        public class SmartVolumeDataResponse
        {
			public bool enabled;
			public float volumeLevel;
			public string loudness;
        }

        public class EqualizerResponse
        {
            public bool enabled;
            public EqualizerFilterResponse filter1;
            public EqualizerFilterResponse filter2;
            public EqualizerFilterResponse filter3;
            public EqualizerFilterResponse filter4;
            public EqualizerFilterResponse filter5;
            public EqualizerFilterResponse filter6;
            public EqualizerFilterResponse filter7;
            public EqualizerFilterResponse filter8;
            public EqualizerFilterResponse filter9;
            public EqualizerFilterResponse filter10;
        }

        public class EqualizerFilterResponse
        {
            public bool enabled;
            public float qFactor;
            public float frequency;
            public float gain;
            public string type;
        }

        public class GroupVirtualSurroundChannelResponse
        {
            public VirtualSurroundChannelResponse frontLeft;
            public VirtualSurroundChannelResponse frontRight;
            public VirtualSurroundChannelResponse center;
            public VirtualSurroundChannelResponse subWoofer;
            public VirtualSurroundChannelResponse rearLeft;
            public VirtualSurroundChannelResponse rearRight;
            public VirtualSurroundChannelResponse sideLeft;
            public VirtualSurroundChannelResponse sideRight;
        }

        public class VirtualSurroundChannelResponse
        {
            public float position;
            public float gain;
        }
    }
}
