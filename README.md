The goal of this project was to create an application that allows you to control the volume of the SteelSeries Sonar application without minimizing the game. The project is still under development, but the current version allows you to control the volume from another device (e.g. Android) located on the same LAN. It is also possible to control from dedicated hardware (e.g. Arduino).

Huge thanks to [@wex](https://github.com/wex) and [@NOoBODDY](https://github.com/NOoBODDY) for finding a way to send commands to SteelSeries Sonar [sonar-rev](https://github.com/wex/sonar-rev).

The project was created in .NET, and its architecture allows for simple extension of the application through extensions. The application itself does not have any graphical interface. It remains running in the System Tray and has only a simple context menu.

To run the application, build CoreApp and the selected extensions. Extensions are loaded from the `Extension` folder next to the CoreApp executable (e.g. `CoreApp\bin\Debug\net10.0-windows\Extension`). Building an extension copies its .dll there automatically (see `Directory.Build.targets`); close the application before building, otherwise the files are locked and the build only shows a warning. To install an extension manually, copy only its main .dll file into that folder (for ExternalCommunication, also ExternalCommunicationShared.dll). Never copy the SteelSeriesSonarCompanion.Shared.* files there, because CoreApp already provides them.

The Hardware extension embeds the System.IO.Ports library inside its own .dll, so it does not need any extra files. The Arduino is detected automatically on any serial port and the port is remembered in `%AppData%\SteelSeriesSonarCompanion\hardware.json`.

Problems are reported with tray notifications and written to `%AppData%\SteelSeriesSonarCompanion\companion.log`.

Future plans:
- Option to enable/disable Sonar features (e.g. Equalizer)
- Option to import equalizer settings from files
- Support for Streamer Mode
- Code improvements
