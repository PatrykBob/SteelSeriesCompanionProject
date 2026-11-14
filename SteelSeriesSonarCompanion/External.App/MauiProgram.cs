using Microsoft.Extensions.Logging;
using SteelSeriesSonarCompanion.External.App.Connection;
using SteelSeriesSonarCompanion.External.App.Layout;
using SteelSeriesSonarCompanion.External.App.Panels;

namespace SteelSeriesSonarCompanion.External.App
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp ()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

			builder.Services.AddSingleton<CompanionConnection>();
			builder.Services.AddSingleton<PanelRegistry>();
			builder.Services.AddSingleton<LayoutStore>();
			builder.Services.AddSingleton<AppShell>();
			builder.Services.AddSingleton<MainPage>();

#if DEBUG
			builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
