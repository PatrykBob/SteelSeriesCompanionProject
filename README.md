The goal of this project was to create an application that allows you to control the volume of the SteelSeries Sonar application without minimizing the game. The project is still under development, but the current version allows you to control the volume from another device (e.g. Android) located on the same LAN. It is also possible to control from dedicated hardware (e.g. Arduino).

Huge thanks to [@wex](https://github.com/wex) and [@NOoBODDY](https://github.com/NOoBODDY) for finding a way to send commands to SteelSeries Sonar [sonar-rev](https://github.com/wex/sonar-rev).

The project was created in .NET, and its architecture allows for simple extension of the application through extensions. The application itself does not have any graphical interface. It remains running in the System Tray and has only a simple context menu.

## Features

### Companion (CoreApp)
- Controls the volume and mute of every Sonar channel: Game, Chat, Media, Aux and Microphone.
- Controls the Windows master volume and mute of the device Sonar outputs to.
- Picks up master volume changes made outside the companion (keyboard volume keys, Windows mixer) and passes them to all extensions and remote apps.
- Finds Sonar automatically and reconnects when SteelSeries GG restarts.
- If a change fails, every device goes back to the last confirmed value.
- Tray menu: extension items, Refresh Audio Device, Start with Windows and Exit.
- Runs as a single instance.
- Problems are reported with tray notifications and written to `%AppData%\SteelSeriesSonarCompanion\companion.log`.

### Remote App extension (ExternalCommunication)
- TCP server for the remote app, found automatically on the LAN through UDP discovery on port 8888.
- Pairing: Remote App > Pair New Device opens a 60-second pairing window; only paired devices can connect.
- Tray items: Pair New Device, Forget Paired Devices, Restart Communication Server.
- Volume changes made on one device are sent to all other connected remote apps.

### Hardware extension
- Controls the Game, Chat and Media volumes with three potentiometers on an Arduino (sketch in `Arduino/`).
- Finds the Arduino automatically on any serial port and remembers it in `%AppData%\SteelSeriesSonarCompanion\hardware.json`.
- Tray item: Reset Port to search for the Arduino again.

### UI extension
- Tray item Show UI opens a window with a volume slider and mute checkbox for every channel.

### Remote app (External.App)
- .NET MAUI app (Android and Windows) with a volume slider and mute toggle for every channel.
- Finds the PC on the LAN, pairs with it from the menu (Pair with PC) and remembers the pairing.
- Sliders stay in sync with changes made on the PC or other devices.

## Building and installing

To run the application, build CoreApp and the selected extensions. Extensions are loaded from the `Extension` folder next to the CoreApp executable (e.g. `CoreApp\bin\Debug\net10.0-windows\Extension`). Building an extension copies its .dll there automatically (see `Directory.Build.targets`); close the application before building, otherwise the files are locked and the build only shows a warning. To install an extension manually, copy only its main .dll file into that folder (for ExternalCommunication, also ExternalCommunicationShared.dll). Never copy the SteelSeriesSonarCompanion.Shared.* files there, because CoreApp already provides them.

## Future plans
- Option to enable/disable Sonar features (e.g. Equalizer)
- Option to import equalizer settings from files
- Support for Streamer Mode
- Chat Mix control for extensions and the remote app
- Code improvements
