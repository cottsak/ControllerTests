# ControllerTests [![Build status](https://ci.appveyor.com/api/projects/status/bii5w35ktguwd29u?svg=true)](https://ci.appveyor.com/project/cottsak/controllertests)

# tl;dr

* Use [`Install-Package mk.ControllerTests`](https://www.nuget.org/packages/mk.ControllerTests/) to quickly build [subcutaneous style tests](http://martinfowler.com/bliki/SubcutaneousTest.html) for MVC/webapi controllers or any arbitrary "top of stack" interface.
* Use `MvcControllerTestBase` to test a [single action](https://github.com/cottsak/ControllerTests/blob/master/ControllerTests.Tests/HomeControllerTests.cs#L47) on a `System.Web.Mvc.Controller`. The idea is that you will set up real state in a store/database and make assertions against it, thus testing from the top[ish] of the stack all the way into a *real* store.
* Use `ApiControllerTestBase` to test behaviour on an `ApiController`. We use the WebApi 2 message pipeline and [send a real](https://github.com/cottsak/ControllerTests/blob/master/ControllerTests.Tests/DeleteICControllerTests.cs#L50) `HttpRequestMessage`. This way we can even test filters and other MVC internals.
* If you have a service/abstraction that say, you use as an entry point for a background task, use that interface with the `AnyControllerTestBase` to test it's methods, and the behaviour all the way into the store.
* Used successfully with real/lcoaldb MSSQL, embedded RavenDB, in-memory Event Store.

# Why?

TODO: confidence, ROI, etc

# Examples

TODO

# Old:

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

