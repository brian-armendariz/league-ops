# C# (.NET) vs Python — LeagueOps Comparison

Purpose: provide a concise, side-by-side comparison to help decide whether to continue building `LeagueOps` in C# or port to Python. Share this with your buddy and use it to decide.

## Project Fit

- **C# (.NET)**
  - Strong fit for Azure Functions (isolated worker), DI, `IOptions<T>`, and typed models.
  - Good for long-term maintainability and larger teams.
  - Easier to integrate with existing .NET libraries and CI build pipelines.

- **Python**
  - Fast prototyping and scripting-friendly. Good for small teams or solo development.
  - Azure Functions supported but packaging and DI differ.
  - Great for integrating with numerous third-party libraries.

## Development Experience

- **C#**
  - Static typing, compiler checks, powerful refactoring in IDEs.
  - Slower iteration (rebuilds) but safer changes.
  - Tooling: Visual Studio / VS Code, `dotnet` CLI, xUnit for tests.

- **Python**
  - Rapid iteration, REPL-friendly, many micro-packages.
  - Need `mypy`/type hints for similar safety.
  - Tooling: VS Code Python extension, `pytest`, virtualenv/Poetry.

## Go

- **Project Fit**
  - Good for small, fast services and CLIs; excellent for networked services and simple concurrency.
  - Useful if you want a single static binary for deployment or require low-latency, minimal-runtime services.

- **Development Experience**
  - Statically typed with fast compilation and a small standard library; simple language surface area.
  - Tooling: VS Code has Go extension (`gopls`), `go test`, `go mod` for dependency management.

- **Libraries & Ecosystem**
  - Strong standard library for HTTP and concurrency; good HTTP clients and smaller ecosystem for niche APIs compared to Python.
  - Discord: community clients exist (e.g., `bwmarrin/discordgo`), but fewer high-level bots than Python.
  - Yahoo Fantasy: unlikely to find a mature Go SDK; you'll implement OAuth + REST with `net/http` or `http.Client` wrappers.

- **Azure Functions**
  - No first-class Go worker; use Azure Functions custom handlers or deploy Go as containerized HTTP endpoints.
  - For serverless, Go often fits better on platforms that support native Go runtimes (e.g., Cloud Run, AWS Lambda with Go runtime).

- **Performance & Scale**
  - Excellent raw performance, small memory footprint, fast startup — good for low-latency endpoints.

- **Concurrency**
  - Goroutines and channels provide ergonomic concurrency; well-suited for concurrent I/O workloads.

- **Packaging & Deployment**
  - Produces a single static binary (if desired) — simplifies deployment and reduces runtime dependencies.

- **Testing & CI**
  - Built-in `go test`, fast toolchain; easy to integrate into CI pipelines.

- **Team & Maintainability**
  - Simpler language but fewer high-level libraries for web APIs compared to Python; great for small, focused services.

- **Recommendations**
  - Consider Go if you need extremely fast, low-footprint services, prefer static binaries, or plan to deploy in containers or platforms that favor Go. It is less convenient if you expect heavy use of Python-specific libraries or the .NET ecosystem.

## Libraries & Ecosystem

- **Discord**: Both have mature options. C# has `Discord.Net`/`DSharpPlus`; Python has `discord.py` and related libs (broader community). Python may be easier for Discord bot features.

- **Yahoo Fantasy**: No widely adopted, official SDK in either ecosystem. Both will require custom OAuth + REST client. Python has more community scripts but quality varies.

- **Other**: Python has richer data and scripting libraries; C# excels in server-side frameworks and performance.

## Azure Functions

- **C#**
  - Native feel with isolated worker, `IOptions<T>`, DI, strongly-typed requests/responses.
  - Build produces deterministic artifacts.

- **Python**
  - Supported runtime; functions are Python callables with `azure-functions` SDK.
  - Packaging uses `requirements.txt` or container images.

## Performance & Scale

- **C#**: Typically lower latency, better cold-start and CPU-bound performance.
- **Python**: Adequate for I/O-bound workloads; may require careful async design.

## Security & Secrets

- Both can use Azure Key Vault and managed identities. Use environment-based config for local dev (`local.settings.json` in C#, `.env` in Python) and never commit secrets.

## Testing & CI

- **C#**: xUnit, integrated build pipelines, strong compile-time checks.
- **Python**: pytest, easier scripting in CI; need to add type checking if desired.

## Team & Maintainability

- **C#**: Better for larger, strongly-typed codebases; easier onboarding for C# devs.
- **Python**: Faster iteration; better if team prefers scripting and quick prototyping.

## Recommendations

- Stick with **C#** if:
  - You already have a working C# codebase (current state of project), want strong typing, and expect to scale/maintain long-term.

- Consider **Python** if:
  - You prefer rapid prototyping, your team is more Python-centric, or you need richer Discord tooling quickly.

## Migration Cost

- Porting involves re-implementing services, tests, and CI. Core business logic converts easy, but DI, build, and deployment scripts must be rewritten.

## Next Steps

- If you pick **C#**: finalize secret storage, add token persistence, and implement CI publishing pipeline.
- If you pick **Python**: I can scaffold a Python Azure Functions project with `yahoo/auth` and `yahoo/callback` endpoints and `requirements.txt`.

---

If you want, I can also create a checklist tailored to your choice (C# or Python) with concrete migration or stabilization steps.

## Git Comparisons & Workflows

This short guide compares common Git transport/auth options, branching models, and best practices for a team working on `LeagueOps`.

- **SSH vs HTTPS**
  - SSH: recommended for developer convenience and security (uses SSH keys). Use `git@github.com:owner/repo.git` remote.
  - HTTPS: works anywhere and integrates with token-based auth (use `gh auth login` or personal access tokens). Useful on CI when SSH keys are harder to manage.

- **Branching models**
  - Trunk-based: short-lived feature branches merged to `main` frequently (fast, simpler CI).
  - Git Flow: feature/release/hotfix branches (more process, useful for strict release cadence).
  - Recommendation: use trunk-based for fast iteration on Functions and automation; adopt protected branches for `main`.

- **Commit messages & conventions**
  - Use conventional commits or at least concise imperative messages: `feat:`, `fix:`, `chore:`, `docs:`.
  - Keep messages focused; reference issue/PR numbers in the body.

- **Secrets & local files**
  - Never commit `local.settings.json`, `.env`, or Key Vault values. Add them to `.gitignore` (already added).
  - Use `git-crypt` or rotate secrets if accidentally committed.

- **Pull requests & code review**
  - Require PR review and passing CI before merge to `main`.
  - Use small PRs with clear descriptions and testing steps.

- **Force-push and history rewriting**
  - Avoid force-pushing shared branches. If you must rewrite history, coordinate with the team and use `--force-with-lease`.

- **Tags and releases**
  - Use annotated tags for releases and attach change notes; consider GitHub Releases and CI-driven changelogs.

- **Large files**
  - Use Git LFS for very large binaries (avoid committing build artifacts; `.gitignore` handles `bin/` and `obj/`).

These practices will keep the repo secure, CI reliable, and collaboration smooth.