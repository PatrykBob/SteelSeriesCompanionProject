using SteelSeriesCompanion.SharedCore;
using System.Diagnostics;

namespace SteelSeriesCompanionUIExtension
{
	public class SteelSeriesCompanionUIExtensionController : BaseSteelSeriesCompanionExtension
	{
		public override void Initialize (ISteelSeriesCompanionCore companionCore)
		{
			base.Initialize(companionCore);
			Trace.WriteLine("Hello from the extension !!!!!!");
			Window1 window1 = new Window1();
			window1.Show();
			window1.Initialize(companionCore);
		}
	}
}
