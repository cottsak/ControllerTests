using Autofac;
using ControllerTests.Web.Controllers;
using Hangfire;
using Owin;

namespace ControllerTests.Web
{
    class BackgroundJobs
    {
        internal static void Configure(IAppBuilder app, IContainer container)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(Config.DatabaseConnectionString)
                .UseAutofacActivator(container);
            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();


            // start jobs
            RecurringJob.AddOrUpdate<IBackgroundService>(service => service.InvokeRun(), Cron.Minutely);
        }
    }
}