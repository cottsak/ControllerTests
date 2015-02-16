namespace ControllerTests.Web.Helpers
{
    public interface IDevAccessChecker
    {
        bool UserHasDevAccess();
    }

    class DevAccessChecker : IDevAccessChecker
    {
        public bool UserHasDevAccess()
        { return false; }
    }
}