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
- Equalizer for every Sonar channel except master (used by the remote app's Equalizer panel and available to extensions):
  - Turn the equalizer on or off and switch between Sonar presets.
  - Edit the filters (type, frequency, gain, Q) and the preamp, then overwrite the current preset or save as a new one. SteelSeries presets are never overwritten.
  - Import an Equalizer APO / [AutoEQ](https://autoeq.app) `ParametricEQ.txt` file (up to 10 filters), as a new preset or over the current one.
- Tray menu: extension items, Import Equalizer Preset (pick a channel, then an AutoEQ `ParametricEQ.txt` file), Refresh Audio Device, Start with Windows and Exit.
- Runs as a single instance.
- Problems are reported with tray notifications and written to `%AppData%\SteelSeriesSonarCompanion\companion.log`.

### Remote App extension (ExternalCommunication)
- TCP server for the remote app, found automatically on the LAN through UDP discovery on port 8888.
- Pairing: Remote App > Pair New Device opens a 60-second pairing window; only paired devices can connect.
- Tray items: Pair New Device, Forget Paired Devices, Restart Communication Server.
- Volume changes made on one device are sent to all other connected remote apps.
- Equalizer changes (on/off, preset, edits, imports) are sent to every connected remote app.

### Hardware extension
- Controls the Game, Chat and Media volumes with three potentiometers on an Arduino (sketch in `Arduino/`).
- Finds the Arduino automatically on any serial port and remembers it in `%AppData%\SteelSeriesSonarCompanion\hardware.json`.
- Tray item: Reset Port to search for the Arduino again.

### UI extension
- Tray item Show UI opens a window with a volume slider and mute checkbox for every channel.

### Remote app (External.App)
- .NET MAUI app (Android and Windows) built from panels you arrange yourself.
- Edit Layout (menu) turns on edit mode:
  - Split any panel with a vertical or horizontal divider, change its type or remove it.
  - Drag dividers to resize panels.
- Separate layouts for portrait and landscape, saved on the device.
- Volume Mixer panel: a volume slider and mute toggle for each channel.
  - Its settings choose which channels it shows and in what order.
  - Sliders can be vertical or horizontal, and the mute buttons can be hidden.
- Equalizer panel for one channel (chosen in its settings):
  - Equalizer on/off switch, preset picker (your presets first) and a frequency response curve.
  - Edit: change the preamp and each filter with a live preview, then save over the current preset or as a new one.
  - Import: pick an AutoEQ `ParametricEQ.txt` file and save it as a new preset or over the current one.
- Finds the PC on the LAN, pairs with it from the menu (Pair with PC) and remembers the pairing.
- Sliders stay in sync with changes made on the PC or other devices.

## Building and installing

To run the application, build CoreApp and the selected extensions. Extensions are loaded from the `Extension` folder next to the CoreApp executable (e.g. `CoreApp\bin\Debug\net10.0-windows10.0.19041.0\Extension`). Building an extension copies its .dll there automatically (see `Directory.Build.targets`); close the application before building, otherwise the files are locked and the build only shows a warning. To install an extension manually, copy only its main .dll file into that folder (for ExternalCommunication, also ExternalCommunicationShared.dll). Never copy the SteelSeriesSonarCompanion.Shared.* files there, because CoreApp already provides them.

## Future plans
- Equalizer controls in the UI extension
- Option to enable/disable other Sonar features (e.g. bass boost, noise cancellation)
- Support for Streamer Mode
- Chat Mix control for extensions and the remote app
- Code improvements
