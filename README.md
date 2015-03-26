# ControllerTests [![Build status](https://ci.appveyor.com/api/projects/status/bii5w35ktguwd29u?svg=true)](https://ci.appveyor.com/project/cottsak/controllertests)

A package to simplify end-to-end testing for MVC and WebApi controllers while supporting legacy db schemas.

### Get the web site running

* clone repo
* build (package restore should download packages)
* create a local db called `ControllerTests.Web` and ensure that the connection string in `web.config` is correct as per your local server
* ensure that the connection string in `Program.cs` is the same as above and F5 the `ControllerTests.MigrateDb` project to migrate your db
* F5 the `ControllerTests.Web` site

### But it's not about the web site

This app demonstrates the ease in which one can create solid end-to-end tests against both MVC and WebApi controllers with minimum infrastructure code. The gold are the tests in `HomeControllerTests.cs` and `DeleteICControllerTests.cs`.

Please leave me feedback via pull requests, issues or hitting me up at [@mattkocaj](https://twitter.com/mattkocaj).

### Package

[`Install-Package mk.ControllerTests`](https://www.nuget.org/packages/mk.ControllerTests/)

**readme todos**

* what are the exciting bits/real value?
* show usage examples first
* explain that NCrunch `[ExclusivelyUses("db-transaction")]` may be required for MSSQL and similar 'transaction isolation' implementations

