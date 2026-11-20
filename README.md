<p align="center">
  <img src="logo.png" alt="Pixel-art tomato" width="128" height="128">
</p>

# Little Pomodoro

[![Build and test](https://github.com/wesleyrondaaa/pomodoro-timer/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wesleyrondaaa/pomodoro-timer/actions/workflows/ci.yml)

A small, portable Windows timer. One executable, no installer, console window,
JavaScript, PowerShell runtime scripts, third-party packages, or administrator access.
Uses the .NET Framework and Windows Forms included with Windows 10 and 11.

## Run

[Download LittlePomodoro.exe](https://github.com/wesleyrondaaa/pomodoro-timer/releases/latest/download/LittlePomodoro.exe)
from the latest release.

Double-click **LittlePomodoro.exe**. You can move just that file to any directory
you own, such as `%LOCALAPPDATA%\LittlePomodoro`. No other app files are needed.
Closing a command prompt will not close the timer. Close its own window to exit.
An unsigned download may trigger Windows SmartScreen; only run binaries you trust.
Building the source yourself is also an option.
Managed Windows machines may block locally built executables through application
control policy. If Windows reports a group-policy block, your administrator must
approve the trusted application; the app does not bypass those protections.

## Build your own EXE

Install a current .NET SDK for development, then open a terminal in the repository:

```powershell
dotnet build -c Release
```

The output is **bin\Release\net48\LittlePomodoro.exe**. Copy just that EXE to your
user tools directory. Close any running copy before replacing it.

[LittlePomodoro.csproj](LittlePomodoro.csproj) tells the SDK to compile
[Pomodoro.cs](Pomodoro.cs) as a Windows GUI app targeting .NET Framework 4.8.
The SDK may download Microsoft's reference assemblies from NuGet on the first
build if the targeting pack is missing. These are build-time only; users need
neither the SDK nor extra runtime packages.

The [manifest](app.manifest) is embedded in the EXE: it requests normal user
permissions (`asInvoker`) and correct display scaling. The EXE is not a script
wrapper or a sandbox.

Only the EXE is needed to run. The remaining files are for development:

- [Pomodoro.cs](Pomodoro.cs): timer and native interface.
- [LittlePomodoro.csproj](LittlePomodoro.csproj): standard .NET build configuration.
- [app.manifest](app.manifest): embedded permissions and display settings.
- [tomato.ico](tomato.ico): original pixel-art tomato icon, embedded in the EXE.
- [logo.png](logo.png): the same tomato artwork for this README.
- [tests/](tests/): test source, excluded from the app.
- This README, [LICENSE](LICENSE), and [.gitignore](.gitignore): documentation,
 open-source terms, and generated-file exclusions.

## Using the timer

- Defaults: 25-minute focus, 5-minute short break, 15-minute long break.
- Every fourth completed focus session prepares a long break.
- The next timer waits for you to press **Start**; it never starts automatically.
- Pause/resume preserves remaining time.
- Reset or switching modes restarts the selected timer without counting a session.
- Change durations from 1 to 180 whole minutes and press **Apply minutes**.
  Applying restarts the current timer.
- **Completion sound** opts into a Windows system sound. Mute and Windows sound
  settings apply.
- **Show window when done** is enabled by default. Completing focus or a break
  restores a minimized timer and requests focus. Disable it to leave the window
  in the background. Windows may restrict focus stealing while you use other apps.
- Closing the app resets settings and session counts. Nothing is saved.

Timing uses a UTC deadline rather than counting ticks, so delayed UI callbacks do
not accumulate drift. Sleep suspends execution: the timer catches up when Windows
resumes, but the sound will be delayed. The app does not prevent sleep or wake the
machine. Manually changing the system clock can affect remaining time.
The UI timer stops completely while idle or paused.

## Privacy

The app makes no network requests and records no telemetry. It installs no
services, scheduled tasks, startup entries, or files outside its own executable.
It does not read your documents or save settings. Windows may retain its normal
diagnostic records.

## Tests

```powershell
dotnet run --project .\tests\LittlePomodoro.Tests.csproj -c Release
```

This builds the app and a dependency-free test executable, then checks timer
behavior, duration validation, native controls, the Windows GUI subsystem,
survival after a launching command prompt exits, and clean shutdown. Tests
briefly open and close windows and need an interactive Windows desktop.

## Sharing and open source

Share **LittlePomodoro.exe** directly, or attach it to a GitHub Release. An installer
or WinGet package is optional, not needed to run it. Generated binaries are ignored
by Git; keep the source and project files in the repository.

See [CONTRIBUTING.md](CONTRIBUTING.md) for development and pull request guidance.
[GitHub Actions](.github/workflows/ci.yml) builds and runs tests on Windows for
pushes and pull requests. It does not publish or deploy anything.

Licensed under [MIT](LICENSE).
