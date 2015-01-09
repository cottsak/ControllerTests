using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MVCControllerTestsWithLocalDb.Web.Models;
using NHibernate;

namespace MVCControllerTestsWithLocalDb.Web
{
    class NhibernateConfig
    {
        internal static ISessionFactory CreateSessionFactory()
        {
            return Fluently
                .Configure()
                .Database(() =>
                    MsSqlConfiguration.MsSql2012.ConnectionString(Config.DatabaseConnectionString))
                .Mappings(mc => mc.FluentMappings.AddFromAssemblyOf<IntegratedCircuit>())
                .BuildSessionFactory();
        }

        internal static Action<ISession> FlushSession = session =>
        {
            if (session.IsDirty())
                session.Flush(); // deletes won't work without explicit .Flush()

            session.Dispose();      // todo: update blog post with this line
        };
    }
}