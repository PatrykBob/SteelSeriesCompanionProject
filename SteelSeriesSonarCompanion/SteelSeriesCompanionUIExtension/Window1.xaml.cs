using SteelSeriesCompanion.SharedCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteelSeriesCompanionUIExtension
{
	/// <summary>
	/// Logika interakcji dla klasy Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		private ISteelSeriesCompanionCore Core { get; set; }

		public Window1 ()
		{
			InitializeComponent();
		}

		public void Initialize (ISteelSeriesCompanionCore core)
		{
			Core = core;
		}

		private void slider_ValueChanged (object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (Core != null)
			{
				Core.SetChannelVolume(SoundChannel.GAME, (float)e.NewValue);
			}
		}
	}
}
