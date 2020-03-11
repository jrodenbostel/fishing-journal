# Fishing Journal Project
The purpose of this project is to serve as a home for learning and experimenting with .NET Core. Eventually, there will be a blog series about some of the struggles related to .NET development on macOS that I've worked through.  Further into the future, this may actually be a private online fishing journal, too.

## Build Status

[![Build Status](https://dev.azure.com/fishing-journal/fishing-journal/_apis/build/status/fishing-journal?branchName=master)](https://dev.azure.com/fishing-journal/fishing-journal/_apis/build/status/fishing-journal?branchName=master)

## Prequisites
* dotnet-sdk (installed via Homebrew: https://formulae.brew.sh/cask/dotnet-sdk)
* Node.js (installed via NVM: https://github.com/nvm-sh/nvm)

## Developer Setup (via CLI)
1. git clone
1. `cd fishing-journal` (move into solution folder)
1. `cd FishingJournal` (move into project folder)
1. `npm install` (install node dependencies)
1. `npm run build` (publish static assets)
1. `cd ..` (move back to solution folder)
1. `dotnet test` (run unit and integration tests, verifying local config)


## Notes
* Sensitive files are excluded from version control.  Example files are checked in where appropriate.
* When setting up a test project that includes integration tests (integration tests including those that involve making requests to a temporary test server and that work against in-memory databases), the Project SDK must be set to "Microsoft.NET.Sdk.Web" in the test project's .csproj file. The default is "Microsoft.NET.Sdk".

## Appendix
### Useful Commands
* drop all database tables: `dotnet ef database drop -f -v -c DefaultContext`
* migrate database to latest version: `dotnet ef database update -c DefaultContext`
* simple scaffolding (requires model): `dotnet aspnet-codegenerator controller -name ShowsController -m <model name> -dc DefaultContext --relativeFolderPath Controllers`
* add a migration: `dotnet ef migrations add <model name> -c DefaultContext`
* run migrations: `dotnet database update -c DefaultContext`

### Useful Links
* Integration test design inspired by: https://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core & https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1
* MSFT Identity scaffold documentation is both incorrect and inflexible for those not wanting to use Bootstrap & JQuery.  The design in this solution is inspired by: https://www.tektutorialshub.com/asp-net-core/asp-net-core-identity-tutorial/#configuring-the-identity-services
