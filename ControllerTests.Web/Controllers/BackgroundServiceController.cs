using System;
using System.Threading;
using FluentNHibernate.Mapping;
using NHibernate;

namespace ControllerTests.Web.Controllers
{
    public interface IBackgroundService
    {
        DateTime? GetLastRunInUtc();
        void InvokeRun();
    }

    public class BackgroundService : IBackgroundService
    {
        private readonly ISession _session;

        public BackgroundService(ISession session)
        {
            _session = session;
        }

        public DateTime? GetLastRunInUtc()
        {
            var bgServiceFlag = _session.Get<BgServiceFlag>(BgServiceFlag.LookupId);
            var flag = bgServiceFlag ?? CreateRecord();
            return flag.LastStarted;
        }

        private BgServiceFlag CreateRecord()
        {
            var flag = new BgServiceFlag { Id = BgServiceFlag.LookupId };
            _session.Save(flag);
            _session.Flush();
            return flag;
        }

        public void InvokeRun()
        {
            // record the start time
            var flag = _session.Get<BgServiceFlag>(BgServiceFlag.LookupId);
            flag = flag ?? CreateRecord();
            flag.LastStarted = DateTime.UtcNow;
            _session.Flush();

            // fake out some real work
            Thread.Sleep(2000);
        }
    }

    public class BgServiceFlag
    {
        internal static int LookupId = 100;

        public virtual int Id { get; set; }
        public virtual DateTime? LastStarted { get; set; }
    }

    class BgServiceFlagMap : ClassMap<BgServiceFlag>
    {
        public BgServiceFlagMap()
        {
            Id(x => x.Id).GeneratedBy.Assigned();
            Map(x => x.LastStarted);
        }
    }
}