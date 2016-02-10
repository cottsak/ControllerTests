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

# Readme todos

* explain that NCrunch `[ExclusivelyUses("db-transaction")]` may be required for MSSQL and similar 'transaction isolation' implementations (this problem might just be about running tests in parallel actually)

