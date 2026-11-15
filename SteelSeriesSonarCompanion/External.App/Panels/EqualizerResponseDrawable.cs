using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class EqualizerResponseDrawable : IDrawable
	{
		public EqualizerData? Equalizer { get; set; }

		private const double MIN_FREQUENCY = 20.0;
		private const double MAX_FREQUENCY = 20000.0;
		private const double DB_RANGE = 15.0;
		private const int POINT_COUNT = 200;
		private const float LEFT_MARGIN = 36f;
		private const float BOTTOM_MARGIN = 20f;
		private const float EDGE_MARGIN = 6f;
		private const float LABEL_FONT_SIZE = 11f;
		private const float CURVE_STROKE_SIZE = 2.5f;

		private static readonly double[] DB_GRID_LINES = [-12, -6, 0, 6, 12];
		private static readonly (double Frequency, string Label)[] FREQUENCY_GRID_LINES = [(100, "100"), (1000, "1k"), (10000, "10k")];
		private static readonly Color ENABLED_DARK_COLOR = Color.FromArgb("#ac99ea");
		private static readonly Color ENABLED_LIGHT_COLOR = Color.FromArgb("#512BD4");
		private static readonly Color DISABLED_COLOR = Color.FromArgb("#6E6E6E");
		private static readonly Color GRID_DARK_COLOR = Color.FromArgb("#404040");
		private static readonly Color GRID_LIGHT_COLOR = Color.FromArgb("#C8C8C8");
		private static readonly Color LABEL_COLOR = Color.FromArgb("#919191");

		public void Draw (ICanvas canvas, RectF dirtyRect)
		{
			bool isDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
			RectF plot = new(dirtyRect.Left + LEFT_MARGIN, dirtyRect.Top + EDGE_MARGIN, dirtyRect.Width - LEFT_MARGIN - EDGE_MARGIN, dirtyRect.Height - BOTTOM_MARGIN - EDGE_MARGIN * 2);

			if (plot.Width <= 0 || plot.Height <= 0)
			{
				return;
			}

			DrawGrid(canvas, plot, isDarkTheme == true ? GRID_DARK_COLOR : GRID_LIGHT_COLOR);

			if (Equalizer != null)
			{
				DrawCurve(canvas, plot, Equalizer, isDarkTheme == true ? ENABLED_DARK_COLOR : ENABLED_LIGHT_COLOR);
			}
		}

		private void DrawGrid (ICanvas canvas, RectF plot, Color gridColor)
		{
			canvas.FontSize = LABEL_FONT_SIZE;
			canvas.FontColor = LABEL_COLOR;

			foreach (double db in DB_GRID_LINES)
			{
				float y = GetY(plot, db);
				canvas.StrokeColor = gridColor;
				canvas.StrokeSize = db == 0 ? 1.5f : 0.75f;
				canvas.DrawLine(plot.Left, y, plot.Right, y);
				canvas.DrawString($"{db:+0;-0;0}", plot.Left - LEFT_MARGIN, y - LABEL_FONT_SIZE, LEFT_MARGIN - 4, LABEL_FONT_SIZE * 2, HorizontalAlignment.Right, VerticalAlignment.Center);
			}

			foreach ((double frequency, string label) in FREQUENCY_GRID_LINES)
			{
				float x = GetX(plot, frequency);
				canvas.StrokeColor = gridColor;
				canvas.StrokeSize = 0.75f;
				canvas.DrawLine(x, plot.Top, x, plot.Bottom);
				canvas.DrawString(label, x - 30, plot.Bottom + 2, 60, BOTTOM_MARGIN, HorizontalAlignment.Center, VerticalAlignment.Center);
			}
		}

		private void DrawCurve (ICanvas canvas, RectF plot, EqualizerData equalizer, Color enabledColor)
		{
			PathF path = new();

			for (int i = 0; i <= POINT_COUNT; i++)
			{
				double frequency = MIN_FREQUENCY * Math.Pow(MAX_FREQUENCY / MIN_FREQUENCY, (double)i / POINT_COUNT);
				double gainDb = Math.Clamp(EqualizerResponseCalculator.GetGainDb(equalizer.Settings, frequency), -DB_RANGE, DB_RANGE);
				PointF point = new(GetX(plot, frequency), GetY(plot, gainDb));

				if (i == 0)
				{
					path.MoveTo(point);
				}
				else
				{
					path.LineTo(point);
				}
			}

			canvas.StrokeColor = equalizer.Enabled == true ? enabledColor : DISABLED_COLOR;
			canvas.StrokeSize = CURVE_STROKE_SIZE;
			canvas.StrokeLineJoin = LineJoin.Round;
			canvas.DrawPath(path);
		}

		private float GetX (RectF plot, double frequency)
		{
			return plot.Left + plot.Width * (float)(Math.Log(frequency / MIN_FREQUENCY) / Math.Log(MAX_FREQUENCY / MIN_FREQUENCY));
		}

		private float GetY (RectF plot, double gainDb)
		{
			return plot.Center.Y - (float)(gainDb / DB_RANGE) * plot.Height / 2f;
		}
	}
}
