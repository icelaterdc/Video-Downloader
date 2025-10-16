# Video Downloader (Windows)

Video Downloader is a simple, reliable, and user-friendly Windows desktop application (WinForms, .NET 8) for downloading videos from direct URLs and, optionally, from streaming sites via yt-dlp integration (planned). The project is developed incrementally — this repository contains the app skeleton and progressive improvements.

### Key features (planned)
- Direct HTTP(S) downloads with progress reporting and resume support.
- Optional integration with yt-dlp for sites like YouTube, Vimeo, Instagram, etc.
- Modern, user-friendly UI with dark mode, drag & drop, clipboard detection, and notifications.
- Single-file Windows executable build via GitHub Actions (Release artifact).
- Safe defaults and filename safety checks.

### Repository layout
- VideoDownloader/           -- WinForms project (source)
  - Resources/               -- Icons and static resources (place app.ico here)
  - Program.cs
  - MainForm.cs
  - VideoDownloader.csproj
- .github/workflows/         -- CI workflow (added later)
- README.md
- CONTRIBUTING.md
- SECURITY.md

### Icon
- Place your application icon as: VideoDownloader/Resources/app.ico
- To use the icon in the build, add (or uncomment) in VideoDownloader.csproj:
  <PropertyGroup>
    <ApplicationIcon>Resources/app.ico</ApplicationIcon>
  </PropertyGroup>

### Build locally
- Install .NET 8 SDK.
- From repository root:
  dotnet publish VideoDownloader/VideoDownloader.csproj -c Release -r win-x64 --self-contained false

### Notes about yt-dlp
- For many streaming sites (YouTube, etc.) we will integrate yt-dlp as an optional helper.
- The app will not bundle third-party binaries by default; instead it can download the latest yt-dlp binary at first run or CI can fetch it to include as an artifact. Licensing and redistribution will be handled explicitly.

### Privacy & Safety
- The application only downloads the content from the URL the user provides. It does not phone home analytics or upload files anywhere.
- For sites that require credentials or special handling, the app will surface warnings and rely on yt-dlp for secure handling where appropriate.
