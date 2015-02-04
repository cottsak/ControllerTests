using System;
using ControllerTests.Web.Models;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace ControllerTests.Web
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

        internal static Action<ISession> CompleteRequest = session =>
            {
                if (session.IsDirty())
                    session.Flush();        // deletes won't work without explicit .Flush()
            };
    }
}