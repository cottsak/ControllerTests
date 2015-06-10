using System;

namespace ControllerTests
{
    static class Constants
    {
        public static readonly Func<Exception> BadBuilderConfigOrderException = () => 
            new InvalidOperationException("Can not configure after the server has been initialised. Always ConfigureServices/SubstituteAndConfigure before using the Session member. This exception also may indicate that your tests are running in parallel and are not thread safe.");
    }
}
