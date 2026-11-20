<p align="center">
  <img src="logo.png" alt="Pixel-art tomato" width="128" height="128">
</p>

# Little Pomodoro

[![Build and test](https://github.com/wesleyrondaaa/little-pomodoro/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wesleyrondaaa/little-pomodoro/actions/workflows/ci.yml)

Little Pomodoro is a small timer for Windows 10 and 11.
It requires .NET Framework 4.8 and uses Windows Forms.
The application has one executable file.
It does not require an installer, a console window, or administrator access.
It does not use JavaScript, PowerShell scripts, or third-party runtime packages.

## Start the application

1. [Download LittlePomodoro.exe](https://github.com/wesleyrondaaa/little-pomodoro/releases/latest/download/LittlePomodoro.exe).
2. Put the file in a folder that you can write to, such as `%LOCALAPPDATA%\LittlePomodoro`.
3. Double-click **LittlePomodoro.exe**.

No other application files are necessary.
You can close the command prompt without stopping the timer.
To stop the application, close its window.

The executable has no digital signature.
Windows SmartScreen can show a warning.
Only run executables from a source that you trust.
You can also build the application from its source code.

Your organization's policy can block the executable.
If Windows reports a policy block, ask your administrator to approve the application.
The application does not bypass these controls.

## Build the executable

1. Install the .NET 10 SDK.
2. Open a terminal in the repository folder.
3. Run this command:

```powershell
dotnet build -c Release
```

The output file is **bin\Release\net48\LittlePomodoro.exe**.
Close the application before you replace its executable.
Copy only the executable to your preferred folder.

The SDK uses [LittlePomodoro.csproj](LittlePomodoro.csproj) to compile [Pomodoro.cs](Pomodoro.cs) for .NET Framework 4.8.
If necessary, the SDK downloads Microsoft's reference assemblies from NuGet.
These files are necessary only to build the application.
Users do not need the SDK or additional runtime packages.

The executable contains the [manifest](app.manifest) and [tomato icon](tomato.ico).
The manifest sets normal user permissions (`asInvoker`) and display scaling.
The application is not a script wrapper or a security sandbox.

The other repository files support development:

- [logo.png](logo.png) contains the tomato image for this README.
- [tests/](tests/) contains test source code.
- [CONTRIBUTING.md](CONTRIBUTING.md) gives instructions for contributors.
- [LICENSE](LICENSE) contains the license terms.
- [.gitignore](.gitignore) excludes generated files from Git.

## Use the timer

The default durations are 25 minutes for focus, 5 minutes for a short break, and 15 minutes for a long break.
Each fourth completed focus session gives you a long break.
The next session does not start automatically.

- Select **Start** to start a session.
- Select **Pause** to stop the timer temporarily.
- Select **Resume** to continue with the remaining time.
- Select **Reset** to reset the selected timer.
- Select a different mode to reset the timer for that mode.

Resetting the timer or changing its mode does not increase the completed session count.
The timer does not run while it is ready or paused.

### Change the settings

1. Set each duration to a whole number from 1 to 180 minutes.
2. Select **Apply minutes**.

This resets the selected timer.

- Select **Completion sound** to enable the Windows system sound.
  Windows volume and sound settings apply.
- Keep **Show window when done** selected to restore the minimized window when a session ends.
  This option is on by default.
  Clear it to leave the window in the background.
  Windows can prevent the application from taking focus.

Closing the application resets all settings and session counts.
The application does not save them.

### Timing limits

The timer uses a UTC deadline instead of counting update events.
A delayed update does not add timing errors.
When Windows resumes from sleep, the timer updates the remaining time.
Sleep delays the completion sound and window restoration.
The application does not prevent sleep or wake the computer.
Changing the system clock can change the remaining time.

## Privacy

The application does not make network requests or record usage data.
It does not install services, scheduled tasks, startup entries, or additional files.
It does not read your documents or save settings.
Windows can keep its normal diagnostic records.

## Run the tests

Run this command from the repository folder:

```powershell
dotnet run --project .\tests\LittlePomodoro.Tests.csproj -c Release
```

This command builds the application and a test executable.
The tests check timing, duration limits, controls, window restoration, and process exit.
They also check that the application runs without a console window.
The tests open and close windows.
They require an interactive Windows desktop.

[GitHub Actions](.github/workflows/ci.yml) builds and tests the application for pushes and pull requests.
It does not publish releases or install the application.

## Share the application

Share **LittlePomodoro.exe**, or attach it to a GitHub Release.
Users do not need an installer or a WinGet package.
Keep the source and project files in the repository.
Do not commit generated executables.

The [MIT license](LICENSE) applies to this project.
