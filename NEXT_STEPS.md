# LocalEats: next steps

## Current status

Steps 1–3 are implemented in this handoff copy. The latest user-reported run after step 3 completed the build and all **34 tests passed** (0 failed, 0 skipped). Three nullable-reference warnings remain in test files. Azure deployment is out of scope.

## Completed improvements

1. **Fail-closed restaurant authorization:** public reads are allowed; only the restaurant owner can update; unsupported resource operations are denied. Restaurant creation has its separate Owner-role and authenticated-user checks.
2. **Layer separation for user context:** `IUserContext` remains in Application; the HTTP-backed `HttpUserContext` implementation is in the API. Missing request, identity, ID, or email claims return `null`.
3. **Single-purpose startup components:** database migration, sample restaurant data, Identity roles, and optional bootstrap Admin provisioning have focused classes coordinated by `StartupDataInitializer`.

## Recommended free test hosting

Use **Cloudflare Pages for the Blazor WebAssembly client** and **MonsterASP.NET for the ASP.NET Core API plus SQL Server**. This keeps the current SQL Server provider and gives the client host a supported way to send CSP response headers. See [Cloudflare's Blazor deployment guide](https://developers.cloudflare.com/pages/framework-guides/deploy-a-blazor-site/) and [Pages custom headers guide](https://developers.cloudflare.com/pages/configuration/headers/).

MonsterASP.NET currently advertises a free test plan with one website, one SQL Server database, 5 GB site storage, 256 MB RAM, and 1 GB database storage. HTTPS on the free tier requires asking support to activate it; the plan has limited traffic and no automatic backups or service guarantees. Treat this as a demo environment and do not use real personal data. Details: [MonsterASP.NET free plan](https://www.monsterasp.net/ASP.NET-Freehosting/).

## Remaining work, in order

### 1. Choose and prepare the GitHub source

The handoff copy has no `.git` metadata. Decide whether LocalEats should go into the existing `look-96/Restaurants.API.Final` repository or a new LocalEats repository. Before pushing, review `.gitignore` and confirm no secrets, local configuration, or generated build output is tracked. The project has not been pushed to GitHub.

### 2. Create the free hosting accounts and resources

- Create a Cloudflare Pages project for the client.
- Create a MonsterASP.NET website and SQL Server database for the API.
- Request HTTPS activation for the MonsterASP.NET free site.
- Save the database connection string in the host's protected configuration; never put its password in `appsettings.json` or GitHub.

### 3. Publish the API and record its URL

Deploy `src/Restaurants.API` to MonsterASP.NET and record the final HTTPS API origin. Configure the production `ConnectionStrings:RestaurantsDb` value using the protected setting supported by the host. Run migrations and confirm the API can reach the database.

### 4. Publish the Blazor client with a production CSP response header

Publish the client:

```powershell
dotnet publish .\src\Restaurants.Client\Restaurants.Client.csproj -c Release -o .\publish\Client
```

Deploy the static files under `publish\Client\wwwroot` to Cloudflare Pages. Add a Cloudflare Pages `_headers` file to that deployed root with a `Content-Security-Policy` response header. Set `connect-src` to the actual API HTTPS origin from step 3; include `frame-ancestors 'none'` in the header. The client `ApiBaseUrl` must use the same API origin. Keep the development localhost addresses out of the production policy.

### 5. Set CORS and verify security in the browser

- Set the API's `AllowedOrigins` to the exact Cloudflare Pages site origin, including `https://` and without a trailing path.
- Open the deployed client and confirm catalog, login, refresh, restaurant/dish actions, and image loading work.
- In browser developer tools, confirm the client document receives the CSP as an HTTP response header and no required script, style, API, font, or image is blocked.
- Confirm API responses include `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, and `Permissions-Policy`.
- On the test environment, send more than 10 Identity requests from one client IP within a minute and confirm HTTP 429. Avoid running this burst against a shared or production service.
- Verify a non-owner cannot edit another user's restaurant or dishes.

### 6. Re-run validation after hosting changes

From the repository root:

```powershell
dotnet build .\Restaurants.API.sln
dotnet test .\Restaurants.API.sln --no-build
```

Record the results and any new warnings here. Earlier successful user runs reported six warnings, then three warnings after step 2/3 updates; the latest step 3 run reported **34 passed, 0 failed, 0 skipped**, with three nullable warnings in tests.

### 7. Final step: current security review and comprehensive README

Do this after the implementation and hosting verification steps above are complete.

- Review the whole application against current ASP.NET Core, Blazor, OWASP, and hosting security guidance. Check authentication and authorization, ownership boundaries, input validation, SQL injection protections, XSS/output encoding, file uploads, secrets/configuration, CORS, CSP, security headers, rate limits, error handling, logging, and deployment settings.
- Fix applicable security gaps, then record any remaining risks or items that depend on a production host/account decision.
- Replace or expand `README.md` into a comprehensive project guide: architecture and layer responsibilities, setup/configuration, database and migrations, authentication, local run/test/deployment instructions, and security notes.
- Include Mermaid diagrams showing the system architecture, every API endpoint grouped by area, and the principal data flows (including authentication, restaurant/dish operations, and image upload/display). Keep the diagrams aligned with the actual routes and code.
- Re-run the build and test commands from step 6 after the review fixes and documentation are complete.

## Further security hardening

These are worthwhile before a real production launch, but are not prerequisites for this free demo:

- Configure trusted forwarded headers and tune rate limits for the actual host/proxy topology.
- Consider an OpenID Connect/BFF design with Secure, HttpOnly cookies before handling real user accounts; the current token session is stored in browser `sessionStorage`, which same-origin scripts can read.
- Re-encode uploaded images server-side and consider malware scanning for public uploads.
- Define Admin recovery and tamper-resistant audit log retention.

Azure deployment remains intentionally deferred.
