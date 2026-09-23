**LeagueOps Functions — Architecture & OAuth flow**

This document explains how the `LeagueOps.Functions` project handles Yahoo OAuth redirects and how it interacts with the other components in the repository.

Mermaid diagram (flow):

```mermaid
flowchart LR
  subgraph Client[Client]
    U[User Browser]
  end

  subgraph Azure[Azure / Hosting]
    FA["Function App\nLeagueOps.Functions"]
  end

  subgraph Functions[Functions]
    YA[YahooAuth function]
    YC[YahooCallback function]
  end

  subgraph Libraries[Internal Libraries]
    YCL["LeagueOps.Yahoo\nYahooFantasyClient / YahooOptions"]
    CORE["LeagueOps.Core\nservices & models"]
    DIS["LeagueOps.Discord\nDiscordWebhookClient"]
  end

  U -- "GET /api/yahoo/auth" --> YA
  YA -- "builds authorization URL using YahooOptions" --> YCL
  YA -- "302 Redirect Location: https://api.login.yahoo.com/..." --> U

  U -- "User authenticates at Yahoo and approves" --> YahooOAuth["Yahoo OAuth Server"]
  YahooOAuth -- "Redirect with code -> /api/yahoo/callback?code=..." --> U
  U -- "GET /api/yahoo/callback?code=..." --> YC

  YC -- "exchange code for tokens / fetch user data" --> YCL
  YCL -- "map/normalize data" --> CORE
  CORE -- "optional notify or post results" --> DIS
  YC -- "HTTP 200 / UI (or redirect to frontend)" --> U

  %% notes
  classDef infra fill:#f9f,stroke:#333,stroke-width:1px;
  class FA infra;
```

Notes
- `YahooAuth` constructs the OAuth authorization URL using `YahooOptions` (ClientId, RedirectUri) and returns a 302 redirect to Yahoo.
- `YahooCallback` parses the OAuth `code` query parameter, uses `YahooFantasyClient` to exchange the code for tokens and/or fetch data, then passes data into `LeagueOps.Core` services.
- `LeagueOps.Core` contains business logic (weekly league processing, models) and may trigger `LeagueOps.Discord` to post notifications.
- Locally the Functions host runs on `http://localhost:7071/api/...`. In Azure the Function App provides the public endpoints.
- Secrets/config (client id/secret, redirect URI) are provided via app settings / `local.settings.json` and bound via `IOptions<YahooOptions>`.

If you want I can add a second diagram showing sequence details (token exchange, storage, and webhooks), or generate a simple sequence diagram for the Yahoo OAuth exchange.

Sequence diagram — Yahoo OAuth exchange

```mermaid
sequenceDiagram
  participant U as "User Browser"
  participant FA as "Function App"
  participant YA as "YahooAuth fn"
  participant YC as "YahooCallback fn"
  participant YAPI as "Yahoo OAuth Server"
  participant YCL as "YahooFantasyClient"
  participant STORAGE as "TokenStore/KeyVault"
  participant CORE as "LeagueOps.Core"
  participant DIS as "DiscordWebhook"

  U->>FA: GET /api/yahoo/auth
  FA->>YA: Invoke YahooAuth
  YA->>YCL: Build authorization URL (client_id, redirect_uri)
  YA-->>U: 302 Redirect -> YAPI (authorization URL)

  U->>YAPI: User authenticates & grants consent
  YAPI-->>U: Redirect to /api/yahoo/callback?code=AUTH_CODE
  U->>FA: GET /api/yahoo/callback?code=AUTH_CODE
  FA->>YC: Invoke YahooCallback

  YC->>YCL: Exchange AUTH_CODE for tokens
  YCL->>YAPI: POST /oauth2/get_token (client_secret, code)
  YAPI-->>YCL: Access token, Refresh token

  YCL->>STORAGE: Persist tokens securely (encrypted)
  YCL->>CORE: Enqueue/process user data (async)
  CORE->>DIS: Send notification or webhook (optional)

  YC-->>U: 200 OK or redirect to app UI

  %% Tokens stored encrypted; refresh handled by background job

```

Notes
- Store tokens in a secure store (Key Vault, encrypted database) — do not log raw tokens.
- Perform token exchange and sensitive operations server-side (Functions) — keep client secrets out of front-end code.
- Use background processing (queue/service) for heavy or long-running tasks after receiving tokens.
- Consider rotating credentials and using managed identities to access secret stores.
