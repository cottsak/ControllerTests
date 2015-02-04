# mem-int-tests-localdb

Demo for mvc controller and webapi in-memory tests against a localdb mdf 

### Get the web site running

* clone repo
* build (package restore should download packages)
* create a local db called `ControllerTests.Web` and ensure that the connection string in `web.config` is correct as per your local server
* ensure that the connection string in `Program.cs` is the same as above and F5 the `ControllerTests.MigrateDb` project to migrate your db
* F5 the `ControllerTests.Web` site

### But it's not about the web site

This app demonstrates the ease in which one can create solid end-to-end tests against both MVC and WebApi controllers with minimum infrastructure code. The gold are the tests in `HomeControllerTests.cs` and `DeleteICControllerTests.cs`.

Please leave me feedback via pull requests, issues or hitting me up at [@mattkocaj](https://twitter.com/mattkocaj).

### Future

I am in the process of extracting the common infrastructure elements of this project into a nuget assembly which I plan to release shortly.