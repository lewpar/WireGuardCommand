# WireGuard Command
A tool designed for generating road-warrior style WireGuard configuration files. It also supports generating commands for each peer through the use of macros. 

This project is not assocated with the WireGuard trademark in any way.

## Build
The project is built on top of [Electron.NET](https://github.com/ElectronNET/Electron.NET) so you will need the [ElectronNET.API](https://www.nuget.org/packages/ElectronNET.API/) installed.

To create a build you must navigate to the `./WireGuardCommand/WireGuardCommand` directory and run the command:
- `electronize build /target win`

This will create a Windows build of the software. Other build targets like MacOS and Linux are supported.

## Dependencies
This project uses the following dependencies:
- **Blazor** - The web framework being used.
- **Electron.NET** - The windowing suite which Blazor is embedded in.
- **HighlightJS** - Used for syntax highlighting code blocks.
- **IPNetwork2** - Easy handling of subnetting.
- **SharpZipLib** - Used for outputting password protected archives.
- **QRCoder** - For generating QR codes for configs.
