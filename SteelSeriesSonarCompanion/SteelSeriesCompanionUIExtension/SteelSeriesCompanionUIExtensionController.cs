using SteelSeriesCompanion.SharedCore;
using System.Diagnostics;

namespace SteelSeriesCompanionUIExtension
{
	public class SteelSeriesCompanionUIExtensionController : BaseSteelSeriesCompanionExtension
	{
		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			PrepareUI(companionCore);
		}

		private void PrepareUI (ISteelSeriesCompanionCore companionCore)
		{
			SteelSeriesCompanionWindow window = new();
			window.Show();
			window.Initialize(companionCore);
		}
	}
}
