using FluentNHibernate.Mapping;

namespace MVCControllerTestsWithLocalDb.Web.Models
{
    public class IntegratedCircuit
    {
        public virtual int Id { get; protected set; }
        public virtual string Code { get; protected set; }
        public virtual string Description { get; protected set; }
    }

    class IntegratedCircuitMap : ClassMap<IntegratedCircuit>
    {
        public IntegratedCircuitMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Code);
            Map(x => x.Description);
        }
    }
}