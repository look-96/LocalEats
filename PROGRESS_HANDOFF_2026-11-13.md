# LocalEats Progress Handoff — 13 November 2026

## Development deployment

- Site: <https://localeats-dev-449901250411.europe-central2.run.app>
- Cloud Run service: `localeats-dev`, region `europe-central2`
- Latest confirmed application commit deployed: `01a017576f9623dd7afcd636bedd8e34048768dc` (duplicate layout fix). The exact serving revision name was not reported after this deployment.
- GitHub Actions run 47 for that exact `main` commit completed successfully: <https://github.com/look-96/LocalEats/actions/runs/43381473392>.
- PostgreSQL migrations, the manual migration job, and the cleanup job completed successfully. Cloud Scheduler is configured for a 15-minute cleanup interval.
- Cloud Run HTTPS forwarding was fixed, and `/api/security/antiforgery` returned HTTP 200.
- Blazor PDB assets are served with the correct MIME type. A prior browser-cache 404 was cleared by a fresh load.
- The Blazor unhandled-error banner now stays hidden during normal unauthenticated responses. The duplicate header/navigation layout on the signed-out owner route was removed; the user confirmed the page now works properly.
- Production API exceptions are logged to console for Cloud Logging.

## Manual verification — completed

1. The site loads; required Blazor PDB asset returns 200; no error banner appears during normal use.
2. Public restaurant listing and sorting return 200 while signed out.
3. Test account registration, sign-in, refresh/session behavior, and sign-out work. Identity's password policy requires a non-alphanumeric character.
4. A regular test account with the Owner role can create a restaurant, upload/display its logo, and create/edit dishes.
5. Cross-account resource authorization rejects a different account's attempted restaurant `PATCH` with `403 Forbidden`.
6. XSS-shaped input is rejected as plain-text validation; SQL-shaped search text is treated as ordinary text and does not expose records.
7. Unsupported image types and images larger than 5 MB are rejected with clear messages; valid image upload works.
8. Signed-out owner-only access is protected. An unauthenticated `GET /api/identity/session` returning `401` is expected; after the layout fix it does not produce duplicate navigation or a persistent error banner.

## Admin and temporary bootstrap cleanup

- A separate bootstrap Admin account was provisioned and verified with the `Admin` role.
- The regular test account was assigned the `Owner` role through the Admin-only identity endpoint.
- Bootstrap email/secret bindings were removed from the Cloud Run service.
- Runtime access to `LOCALEATS_BOOTSTRAP_ADMIN_PASSWORD` was revoked, and version 1 was disabled. Do not re-enable unless another bootstrap operation is intentionally required.
- Do not store passwords, cookies, antiforgery tokens, or destructive-command credentials here.

## Project constraints and next action

- Google Cloud operations should use `gcloud` CLI commands, not the console UI.
- Keep one file responsible for one focused concern when changing code.
- Build, tests, and Cloud Run deployment have been user-run and reported successful. Before another code change, check `git status` and the current GitHub Actions run for `main`.
- No known manual browser check from the current walkthrough remains. The next useful handoff task is to review Google Cloud spend/alerts and the production plan, then decide whether to configure a separate Production environment or keep LocalEats Development-only for now.

## Google Cloud CLI context

- Project: `project-2d1307cc-ca8a-4589-b39`
- Region: `europe-central2`
- Cloud Shell build checkout: `~/LocalEats-cloud-build`
- Use the deployed URL above for browser checks.
