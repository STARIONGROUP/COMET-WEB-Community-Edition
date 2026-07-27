# End-to-End (Playwright) tests

Playwright-based end-to-end tests that drive a **running** COMET WEB application against a **running** COMET Web
Services server (issue #822).

They live in the `COMETwebapp.Tests.IntegrationTests` namespace and carry the `[Category("EndToEnd")]` attribute, so
the unit-test CI filter (`FullyQualifiedName!~IntegrationTests`) excludes them. The dedicated
`.github/workflows/end-to-end-tests.yml` workflow runs them.

## Design

- **Page Object Model** (`PageModels/`) keeps selectors in one place — a UI change is a one-line fix there, not a
  change scattered across fixtures. This is the coverage-vs-maintenance trade-off asked for in #822.
- **One page object + one fixture per application page.** Every application has a `<Application>PageModel` (in
  `PageModels/`, deriving from `ApplicationPageModel`) that exposes that page's landmark and its main elements as
  `ILocator` members, and a `<Application>TestFixture` deriving from `ApplicationPageTestBase<TPageModel>`. The base
  logs in **once**, opens the application as a tab in `[OneTimeSetUp]`, builds the page object (exposed as
  `this.PageModel`), waits for it to load, and runs the inherited `VerifyPageIsDisplayed` smoke test (page loaded + no
  Blazor error). This gives every page a defined test **and** a ready starting point for feature tests: add a `[Test]`
  to the matching fixture and use `this.PageModel.<Element>` — the page is already open and its selectors are already
  captured. Add missing elements to the page model, not to the test. A new application needs a page model + a ~12-line
  fixture (`ApplicationName` + `CreatePageModel`). The three toolbar pages share `ToolbarApplicationPageModel`
  (`SelectSectionAsync` + a `Sections` list).
- **`LoginTestFixture`** uses a fresh browser per test (it needs an unauthenticated start): landing page, login, logout.
- Besides the smoke test, each fixture carries a **feature test** as a worked example to build on: Model Editor opens
  an element → details panel; Parameter Editor expands/collapses an element group; System Representation navigates the
  product tree; Model/Subscription Dashboards assert their charts/tables render; Requirement Management opens a spec in
  the document viewer; the toolbar apps (Engineering Model / Reference Data / Server Administration) navigate every
  section; Book Editor opens the add-book dialog; and `LoginTestFixture` covers refresh/logout. Data-dependent tests
  (e.g. opening a specification) use `Assume.That(...)` so they go **inconclusive**, not red, when the seed lacks data.
- Read-only by default: the suite does not persist changes. The Book Editor "add" test opens the dialog and cancels
  (no write); add a full create+delete flow only if you want it — it is designed for the ephemeral CI database.
- Most of a fixture's wall-clock is its one-time login + open (real Blazor Server ↔ COMET server round-trips), which is
  amortised across all of that fixture's tests. Selectors live only in the page objects — a UI change is a one-line fix.
- **Prefer `id` selectors.** Page objects target elements by `id` wherever one exists. When a page element that a
  test needs has no id, add a stable `id`/`Id` to the production component (it is a purely additive DOM attribute — no
  layout or behaviour change) rather than reaching for a brittle CSS-class or text selector, then use that id in the
  page object. Dynamic collections (tree nodes, toolbar items) are the exception — they are matched by class/text.
- Tests select the **first** available model/domain/iteration, so they do not depend on specific seed data.
- The flow stays inside the authenticated Blazor Server circuit. A full-page navigation (e.g. to `/Tabs`) starts a new,
  unauthenticated circuit and bounces back to login — so after login the tests use the "Open Tab" card already present
  on the home page rather than navigating.
- All URLs and credentials come from environment variables (with local defaults), so the same suite runs unchanged
  locally and in the containerised CI pipeline.

| Variable | Default | Purpose |
|---|---|---|
| `COMETWEBAPP_URL` | `http://localhost:8080` | The application under test |
| `COMET_SERVER_URL` | `http://localhost:5000` | The COMET Web Services server |
| `COMET_USERNAME` / `COMET_PASSWORD` | `admin` / `pass` | Login credentials |
| `COMET_E2E_HEADLESS` | `true` | Set to `false` to watch the browser |

## Running locally

```bash
# 1. Start the COMET Web Services stack (pre-seeded test database, server on :5000)
docker compose -f .github/e2e/docker-compose.e2e.yml up -d

# 2. Build the test project, then install the Playwright browsers (once)
dotnet build COMETwebapp.Tests/COMETwebapp.Tests.csproj
pwsh COMETwebapp.Tests/bin/Debug/net10.0/playwright.ps1 install chromium chromium-headless-shell

# 3. Start the app (listens on http://localhost:8080)
dotnet run --project COMETwebapp/COMETwebapp.csproj

# 4. Run the suite
dotnet test COMETwebapp.Tests/COMETwebapp.Tests.csproj --filter "TestCategory=EndToEnd"
```

Set `COMET_E2E_HEADLESS=false` to watch the browser drive the app.

## Coverage & SonarQube

These are **behavioural** tests, not a coverage vehicle. They run in the test process but drive a **separately
running** application process, so a normal coverlet/`dotnet-coverage` run only instruments the test-side code (the page
models), **not** the Blazor application code exercised in the other process. That is why the SonarQube coverage number
comes from the unit/bunit suite (`CodeQuality.yml`), and the e2e workflow is a separate pass/fail gate.

If you ever want to see which application code an e2e run touches, instrument the *app* process instead of the test
process — run the app under the collector and let it flush on graceful shutdown:

```bash
dotnet-coverage collect "dotnet COMETwebapp/bin/Debug/net10.0/COMETwebapp.dll" -f xml -o e2e-coverage.xml
# ...run the suite against it, then SIGTERM the app so the collector writes the file...
```

`dotnet-coverage merge` can then combine it with the unit `coverage.xml` and feed the result to Sonar
(`sonar.cs.vscoveragexml.reportsPaths`). This is intentionally **not** wired up by default: it measures a different
process and mixing it into the new-code coverage gate tends to mislead more than it helps.
