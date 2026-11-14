using SteelSeriesSonarCompanion.Shared.Core;

namespace SteelSeriesSonarCompanion.External.App.Layout
{
	public class LayoutStore
	{
		private const string LAYOUT_PREFERENCE_KEY_FORMAT = "Layout.{0}";

		public LayoutNode Load (LayoutOrientation orientation)
		{
			string? json = Preferences.Get(GetPreferenceKey(orientation), null);

			if (json == null)
			{
				return new LayoutDocument().Root;
			}

			LayoutDocument? document = ParseDocument(json);

			if (document == null || document.Root == null)
			{
				return new LayoutDocument().Root;
			}

			SanitizeWeights(document.Root);
			return document.Root;
		}

		public void Save (LayoutOrientation orientation, LayoutNode root)
		{
			LayoutDocument document = new()
			{
				Root = root
			};

			Preferences.Set(GetPreferenceKey(orientation), JsonConverter.ConvertToJSON(document));
		}

		private LayoutDocument? ParseDocument (string json)
		{
			try
			{
				return JsonConverter.ConvertFromJSON<LayoutDocument>(json);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private void SanitizeWeights (LayoutNode node)
		{
			if (double.IsFinite(node.Weight) == false || node.Weight <= 0)
			{
				node.Weight = 1;
			}

			if (node.IsSplit == false)
			{
				return;
			}

			for (int i = 0; i < node.ChildCollection!.Count; i++)
			{
				SanitizeWeights(node.ChildCollection[i]);
			}
		}

		private string GetPreferenceKey (LayoutOrientation orientation)
		{
			return string.Format(LAYOUT_PREFERENCE_KEY_FORMAT, orientation);
		}
	}
}
