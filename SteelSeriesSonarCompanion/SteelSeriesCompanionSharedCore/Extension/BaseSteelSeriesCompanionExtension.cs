namespace SteelSeriesCompanion.SharedCore
{
	public abstract class BaseSteelSeriesCompanionExtension
	{
		protected ISteelSeriesCompanionCore? CompanionCore { get; private set; }

		public abstract ToolStripMenuItem GetExtensionMenuItem ();

		public virtual void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			CompanionCore = companionCore;
		}
	}
}
