using Newtonsoft.Json.Linq;
using SteelSeriesSonarCompanion.External.App.Connection;

namespace SteelSeriesSonarCompanion.External.App.Panels
{
	public class EmptyPanel : BasePanel
	{
		private const string PLACEHOLDER_TEXT = "Empty panel - pick a type in Edit Layout";

		public EmptyPanel (CompanionConnection connection, JObject? settings) : base(connection, settings)
		{
			Content = new Label
			{
				Text = PLACEHOLDER_TEXT,
				TextColor = Colors.Gray,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Center
			};
		}
	}
}
