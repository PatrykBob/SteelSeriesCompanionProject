using System.Globalization;
using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class EqualizerFilterEditorRow
	{
		public event Action Changed = delegate { };

		public Grid Layout { get; }
		private CheckBox EnabledCheckBox { get; }
		private Picker TypePicker { get; }
		private Entry FrequencyEntry { get; }
		private Entry GainEntry { get; }
		private Entry QFactorEntry { get; }

		private static readonly EqualizerFilterType[] FILTER_TYPES = Enum.GetValues<EqualizerFilterType>();
		private static readonly string[] FILTER_TYPE_NAMES = ["Peaking", "Low shelf", "High shelf", "Low pass", "High pass", "Notch"];
		private static readonly Color INVALID_COLOR = Colors.IndianRed;

		public EqualizerFilterEditorRow (EqualizerFilter filter)
		{
			EnabledCheckBox = new CheckBox { IsChecked = filter.Enabled, VerticalOptions = LayoutOptions.Center };
			TypePicker = new Picker { ItemsSource = FILTER_TYPE_NAMES, SelectedIndex = Array.IndexOf(FILTER_TYPES, filter.Type) };
			FrequencyEntry = CreateNumberEntry(filter.Frequency);
			GainEntry = CreateNumberEntry(filter.Gain);
			QFactorEntry = CreateNumberEntry(filter.QFactor);

			Layout = new Grid { ColumnSpacing = 12, ColumnDefinitions = CreateColumnDefinitions() };
			Layout.Add(EnabledCheckBox, 0, 0);
			Layout.Add(TypePicker, 1, 0);
			Layout.Add(FrequencyEntry, 2, 0);
			Layout.Add(GainEntry, 3, 0);
			Layout.Add(QFactorEntry, 4, 0);

			EnabledCheckBox.CheckedChanged += (sender, e) => Changed();
			TypePicker.SelectedIndexChanged += (sender, e) => Changed();
		}

		public static ColumnDefinitionCollection CreateColumnDefinitions ()
		{
			return
			[
				new ColumnDefinition(new GridLength(50)),
				new ColumnDefinition(new GridLength(2, GridUnitType.Star)),
				new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
				new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
				new ColumnDefinition(new GridLength(1, GridUnitType.Star))
			];
		}

		public bool TryGetFilter (out EqualizerFilter? filter)
		{
			bool isFrequencyValid = TryReadNumber(FrequencyEntry, EqualizerLimits.MIN_FREQUENCY, EqualizerLimits.MAX_FREQUENCY, out double frequency);
			bool isGainValid = TryReadNumber(GainEntry, EqualizerLimits.MIN_GAIN, EqualizerLimits.MAX_GAIN, out double gain);
			bool isQFactorValid = TryReadNumber(QFactorEntry, EqualizerLimits.MIN_Q_FACTOR, EqualizerLimits.MAX_Q_FACTOR, out double qFactor);
			bool isTypeValid = TypePicker.SelectedIndex >= 0;

			if (isFrequencyValid == false || isGainValid == false || isQFactorValid == false || isTypeValid == false)
			{
				filter = null;
				return false;
			}

			filter = new EqualizerFilter(EnabledCheckBox.IsChecked, FILTER_TYPES[TypePicker.SelectedIndex], frequency, gain, qFactor);
			return true;
		}

		public static bool TryReadNumber (Entry entry, double min, double max, out double value)
		{
			string text = (entry.Text ?? string.Empty).Trim().Replace(',', '.');
			bool isValid = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) == true && value >= min && value <= max;
			entry.TextColor = isValid == true ? null : INVALID_COLOR;
			return isValid;
		}

		public static string FormatNumber (double value)
		{
			return value.ToString("0.###", CultureInfo.InvariantCulture);
		}

		private Entry CreateNumberEntry (double value)
		{
			Entry entry = new()
			{
				Text = FormatNumber(value),
				Keyboard = Keyboard.Numeric,
				HorizontalTextAlignment = TextAlignment.End
			};

			entry.TextChanged += (sender, e) => Changed();
			return entry;
		}
	}
}
