# ASP.NET Core starter for Wodby

A production-oriented web application for the [Wodby ASP.NET Core service](https://github.com/wodby/service-aspnetcore) and [ASP.NET Core stack](https://github.com/wodby/stack-aspnetcore).

It uses only ASP.NET Core and demonstrates:

- a minimal API and static files from `wwwroot`
- a health check endpoint for deployment checks
- client addresses and schemes from Wodby's gateway (forwarded headers)
- host filtering for the environment's domains from `WODBY_HOSTS`
- locked NuGet restores, integration tests, and Wodby CI

## Local development

With the [.NET 10 SDK](https://dotnet.microsoft.com/download):

```shell
dotnet test --solution App.slnx
dotnet run --project src/App
```

Open <http://localhost:8080>. Useful endpoints are:

- `/` — the static landing page in `src/App/wwwroot`
- `/api/status` — a JSON response from a minimal API
- `/healthz` — the deployment health endpoint

## Project structure

`src/App` is the web application and `tests/App.Tests` its integration tests, which start the application in memory
with `WebApplicationFactory`. `Directory.Build.props` sets the target framework for both and turns on NuGet lock
files: after changing a package reference, run `dotnet restore App.slnx` and commit the updated `packages.lock.json`
files. Builds and Wodby CI restore in locked mode and fail when a lock file is out of date.

`Dockerfile` publishes `src/App` with the .NET SDK and copies the result into the Wodby service image, which has the
ASP.NET Core runtime but no SDK. The Wodby pipeline in `.wodby/pipeline.yml` runs the tests in the matching SDK
image, builds, pushes and deploys; `.wodby/post-deployment.yml` checks `/healthz` after each deployment.

## On Wodby

- The application listens on port 8080 (`ASPNETCORE_HTTP_PORTS`). `launchSettings.json` is used only by
  `dotnet run` on your machine.
- Wodby runs it as `Development` in development environments and as `Production` elsewhere.
- With `WODBY_HOSTS` set, requests for other host names get `400 Bad Request`. The app service's own name
  (`WODBY_APP_SERVICE_NAME`) and `localhost` stay allowed for checks inside the environment; add more names to
  `AllowedHosts` in `appsettings.json`, separated by `;`.
- The health check covers the application only. PostgreSQL, Valkey and SMTP links reach the application as `DB_*`,
  `REDIS_*` and `SMTP_*` variables when you enable them in the stack; build connection strings from them with the
  client's connection string builder.
- The application stores no files. If you add cookie authentication or antiforgery, persist
  [Data Protection keys](https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview)
  to Redis or a database, or cookies stop working after a deployment and across replicas.

In a Wodby development workspace, the checkout runs with `dotnet watch`: changes are applied as you save them,
without a deployment.
