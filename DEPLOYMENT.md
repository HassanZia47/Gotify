# Goatify Deployment

This setup targets:

- Blazor WebAssembly client on GitHub Pages
- ASP.NET Core API on Azure App Service
- Azure SQL Database free tier

## GitHub Pages client

1. In GitHub, set Pages source to GitHub Actions.
2. Optional: add a repository variable named `API_BASE_URL` if you want to override the checked-in default. The current default is `https://goatify-api-hbz-cuaef0esewbvarhs.southeastasia-01.azurewebsites.net/`.
3. Push to `main`. The workflow publishes `Goatify.UI/Goatify.UI.Client`.

The workflow rewrites the Blazor `<base>` tag to the repository path, adds `.nojekyll` so GitHub Pages serves Blazor's `_framework` and `_content` folders, and publishes the static `wwwroot` output.

If GitHub Pages shows "There isn't a GitHub Pages site here", check the latest `Deploy Blazor client to GitHub Pages` run in the repository Actions tab. The first successful run creates the site. A failed run commonly means Pages is not set to GitHub Actions.

## API environment variables

Set these in the Azure App Service configuration instead of putting secrets in `appsettings.json`:

```bash
ConnectionStrings__DefaultConnection="Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
Jwt__Key="<at-least-32-character-random-secret>"
Jwt__Issuer="GoatifyAPI"
Jwt__Audience="GoatifyClient"
Cors__AllowedOrigins__0="https://<github-user>.github.io"
ASPNETCORE_ENVIRONMENT="Production"
```

If your GitHub Pages URL is project-based, CORS still uses only the origin, for example `https://<github-user>.github.io`, not `/Goatify`.

## Azure App Service outline

1. Add these repository variables/secrets in GitHub:

```bash
AZURE_API_APP_NAME="goatify-api-hbz"
AZURE_API_PUBLISH_PROFILE="<downloaded Azure publish profile>"
```

2. Publish the API:

```bash
dotnet publish Goatify.API/Goatify.API.csproj -c Release -o publish/api
```

3. The `Deploy API to Azure App Service` workflow deploys `publish/api`.
4. Test `https://goatify-api-hbz-cuaef0esewbvarhs.southeastasia-01.azurewebsites.net/health`.

## Database

Create an Azure SQL free database, then run EF migrations against it:

```bash
export ConnectionStrings__DefaultConnection="Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Persist Security Info=False;User ID=<user>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
dotnet ef database update --project Goatify.Infrastructure --startup-project Goatify.API
```
