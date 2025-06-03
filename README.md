The goal of this project was to create an application that allows you to control the volume of the SteelSeries Sonar application without minimizing the game. The project is still under development, but the current version allows you to control the volume from another device (e.g. Android) located on the same LAN. It is also possible to control from dedicated hardware (e.g. Arduino).

Huge thanks to [@wex](https://github.com/wex) and [@NOoBODDY](https://github.com/NOoBODDY) for finding a way to send commands to SteelSeries Sonar [sonar-rev](https://github.com/wex/sonar-rev).

The project was created in .NET, and its architecture allows for simple extension of the application through extensions. The application itself does not have any graphical interface. It remains running in the System Tray and has only a simple context menu.

To run the application, build CoreApp and the selected extensions and place them in the Extensions folder (only the main .dll file).

Unfortunately, the current version does not include error handling, so most problems have to be solved by restarting the application.

Future plans:
- Error handling
- Option to enable/disable Sonar features (e.g. Equalizer)
- Option to import equalizer settings from files
- Support for Streamer Mode
- Code improvements

Known problems:
- Currently only the default SteelSeries Sonar installation folder is supported
- System.IO.Ports library is added to CoreApp, because without it, the connection to Arduino did not work
- The ExternalCommunication extension also requires ExternalCommunicationShared.dll to be placed in the Extensions folder in order to work properly
