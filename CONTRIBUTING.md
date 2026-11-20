# Contributing

Small, focused contributions are welcome. For larger changes, open an issue first
so we can agree on the scope before you spend time implementing them.

## Development

Use Windows 10 or 11, .NET Framework 4.8, and the .NET 10 SDK.
From the repository directory:

```powershell
dotnet build -c Release
dotnet run --project .\tests\LittlePomodoro.Tests.csproj -c Release
```

The app is `bin\Release\net48\LittlePomodoro.exe`. Close running copies before
rebuilding. Tests briefly open and close windows; run them on a Windows desktop.
The test runner is a console application, not a `dotnet test` project. A failed
check exits with a nonzero code so local and CI failures are visible.

## Pull requests

- Keep changes focused and explain the behavior being changed.
- Preserve the single-EXE app, offline operation, and normal-user permissions.
- Avoid third-party runtime dependencies, installers, and generated UI projects.
- Follow the existing C# style; add focused tests for behavior you change.
- Run the build and tests, and report any checks you could not run.
- Do not commit generated EXEs, `bin`/`obj` folders, credentials, or personal data.

GitHub Actions builds and runs the tests on Windows for pushes and pull requests.
It does not publish releases or install the app. Workflow action commits are
recorded in [actions.lock](.github/workflows/actions.lock). Verify upstream
releases before changing action versions, then run `gh actions-lock` and
`gh actions-lock --verify` with an authenticated GitHub CLI.

## Bug reports and security

For ordinary bugs, include reproduction steps, expected behavior, Windows version,
and relevant error messages. Review screenshots and logs for personal information
before sharing them.

Do not post exploit details or credentials in a public issue. Use GitHub's private
vulnerability reporting on the repository's Security tab if enabled; otherwise,
ask the maintainer for a private reporting channel without disclosing details.

Contributions are licensed under this project's [MIT license](LICENSE).
