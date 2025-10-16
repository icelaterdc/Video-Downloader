# Contributing

Thank you for considering contributing to Video Downloader! We appreciate any help — bug reports, suggestions, documentation improvements, and code contributions are all welcome.

How to contribute
1. Fork the repository and create a feature branch:
   git checkout -b feat/your-feature
2. Keep changes focused and small; one logical change per PR.
3. Run existing tests (if any) and add tests for new features where appropriate.
4. Follow the project's code style and naming conventions.

Code style
- C# (.NET 8) conventions: use PascalCase for types and methods, camelCase for local variables.
- Keep methods short and single-responsibility.
- Make UI updates on the UI thread. Use async/await for I/O.

Issues
- Open an issue for bugs or feature requests. Provide steps to reproduce for bugs and as much context as possible (OS version, .NET SDK version, sample URL if applicable).

Pull Requests
- Base PRs against main (or the branch noted in the contributing guide).
- Use a descriptive title and provide a summary of the changes.
- Link related issues in the PR description.
- Keep CI green — the project will run a build workflow for pull requests.
- Maintainers may request changes before merging.

Testing
- Prefer unit tests for core logic and integration tests for network-related code where feasible.
- In UI code, prefer separation of logic from UI components to allow testing.

Security and sensitive data
- Do not commit secrets, API keys, or credentials. Use environment variables or GitHub Secrets for CI.
- Follow the repository's SECURITY.md if you discover a vulnerability.

Communication
- Be respectful and constructive. If a maintainer requests changes, please respond and update your PR.

Thank you for contributing!
