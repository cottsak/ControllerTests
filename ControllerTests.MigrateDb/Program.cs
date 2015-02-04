using System;
using System.Linq;
using System.Reflection;
using DbUp;

namespace ControllerTests.MigrateDb
{
    class Program
    {
        internal static int Main(string[] args)
        {
            var connectionString =
                args.FirstOrDefault() ?? @"Server=.\sqlexpress; Database=ControllerTests.Web; Trusted_connection=true";

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}
