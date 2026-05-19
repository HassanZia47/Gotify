# Goatify Deployment

This setup targets:

- Blazor WebAssembly client on GitHub Pages
- ASP.NET Core API on a Linux VPS
- Azure SQL Database free tier

## GitHub Pages client

1. In GitHub, set Pages source to GitHub Actions.
2. Add a repository variable named `API_BASE_URL`, for example `https://api.example.com/`.
3. Push to `main`. The workflow publishes `Goatify.UI/Goatify.UI.Client`.

The workflow rewrites the Blazor `<base>` tag to the repository path, adds `.nojekyll` so GitHub Pages serves Blazor's `_framework` and `_content` folders, and publishes the static `wwwroot` output.

If GitHub Pages shows "There isn't a GitHub Pages site here", check the latest `Deploy Blazor client to GitHub Pages` run in the repository Actions tab. The first successful run creates the site. A failed run commonly means Pages is not set to GitHub Actions or the `API_BASE_URL` repository variable is missing.

## API environment variables

Set these on the VPS instead of putting secrets in `appsettings.json`:

```bash
ConnectionStrings__DefaultConnection="Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
Jwt__Key="<at-least-32-character-random-secret>"
Jwt__Issuer="GoatifyAPI"
Jwt__Audience="GoatifyClient"
Cors__AllowedOrigins__0="https://<github-user>.github.io"
ASPNETCORE_URLS="http://127.0.0.1:5000"
ASPNETCORE_ENVIRONMENT="Production"
```

If your GitHub Pages URL is project-based, CORS still uses only the origin, for example `https://<github-user>.github.io`, not `/Goatify`.

## VPS outline

1. Install the .NET 8 ASP.NET Core runtime.
2. Publish the API:

```bash
dotnet publish Goatify.API/Goatify.API.csproj -c Release -o publish/api
```

3. Copy the published API to the VPS.
4. Run the API as a systemd service on `127.0.0.1:5000`.
5. Put Nginx in front of it with HTTPS from Let's Encrypt.
6. Test `https://api.example.com/health`.

## Database

Create an Azure SQL free database, then run EF migrations against it:

```bash
export ConnectionStrings__DefaultConnection="Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
dotnet ef database update --project Goatify.Infrastructure --startup-project Goatify.API
```
