namespace SteelSeriesSonarCompanion.Shared.Core
{
	public class EqualizerPreset
	{
		public string Id { get; }
		public string Name { get; }
		public bool IsFactoryPreset { get; }

		public EqualizerPreset (string id, string name, bool isFactoryPreset)
		{
			Id = id;
			Name = name;
			IsFactoryPreset = isFactoryPreset;
		}
	}
}
