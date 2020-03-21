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
* migrate database to latest version (i.e. run migrations): `dotnet ef database update -v -c DefaultContext`
* simple scaffolding (requires model): `dotnet aspnet-codegenerator controller -name <controller name> -m <model name> -dc DefaultContext --relativeFolderPath Controllers`
* add a migration: `dotnet ef migrations add <model name> -c DefaultContext`


### Useful Links
* Integration test design inspired by: https://www.dotnetcurry.com/aspnet-core/1420/integration-testing-aspnet-core & https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-3.1
* MSFT Identity scaffold documentation is both incorrect and inflexible for those not wanting to use Bootstrap & JQuery.  The design in this solution is inspired by: https://www.tektutorialshub.com/asp-net-core/asp-net-core-identity-tutorial/#configuring-the-identity-services


# Linux (Ubuntu) Instructions

## Prerequisites
* npm: `sudo apt install npm`
* .NET Core SDK and .NET Core Runtime (instructions: https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1910)
* Entity Framework: `dotnet tool install --global dotnet-ef`
* SQL Server (instructions: https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15)  
  * As the instructions say, verify that the server is running by executing `systemctl status mssql-server --no-pager` after installation. You should get output similar to this (timestamp and PID will vary):
  ```bash
  ● mssql-server.service - Microsoft SQL Server Database Engine
   Loaded: loaded (/lib/systemd/system/mssql-server.service; enabled; vendor preset: enabled)
   Active: active (running) since Sat 2020-03-21 15:37:07 CDT; 47s ago
     Docs: https://docs.microsoft.com/en-us/sql/linux
   Main PID: 8018 (sqlservr)
    Tasks: 150
   CGroup: /system.slice/mssql-server.service
           ├─8018 /opt/mssql/bin/sqlservr
           └─8056 /opt/mssql/bin/sqlservr
  ```
* Verify that you can connect to your database by running `sqlcmd -S localhost -U SA -P '<YourPassword>'` on your shell.

* ~~SQLite: `sudo apt-get install libsqlite3-dev`~~

## Developer setup (via CLI)

1. git clone <project url>
2. `cd fishing-journal` (move into solution folder)
3. `cd FishingJournal` (move into project folder)
4. `dotnet add package Microsoft.EntityFrameworkCore.Sqlite` (Install Entity Framework Core)
4. `cp appsettings.json.example appsettings.json`

## References

1. https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1910)
2. https://docs.microsoft.com/en-us/ef/core/get-started/?tabs=netcore-cli
3. If you get "`Cannot find command dotnet ef`" error:  
    a. https://github.com/dotnet/efcore/issues/15448  
    b. https://ardalis.com/dotnet-ef-does-not-exist
5. https://dotnet.today/en/entity-framework-7/platforms/coreclr/getting-started-linux.html
6. https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15
