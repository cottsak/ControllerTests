# mem-int-tests-localdb

Demo for mvc controller and webapi in-memory tests against a localdb mdf 

### Get the web site running

* clone repo
* build (package restore should download packages)
* create a local db called `MVCControllerTestsWithLocalDb` and ensure that the connection string in `web.config` is correct as per your local server
* ensure that the connection string in `Program.cs` is the same as above and F5 the `MVCControllerTestsWithLocalDb.Db` project to migrate your db
* F5 the `MVCControllerTestsWithLocalDb.Web` site

### But it's not about the web site

This app demonstrates the ease in which one can create solid end-to-end tests against both MVC and WebApi controllers with minimum infrastructore code. The gold are the tests in `HomeControllerTests.cs` and `DeleteICControllerTests.cs` along with the supporting infrastructure in `MVCControllerTest.cs` and `ApiControllerTest.cs`.

Please leave me feedback via pull requests, issues or hitting me up at [@mattkocaj](https://twitter.com/mattkocaj).
