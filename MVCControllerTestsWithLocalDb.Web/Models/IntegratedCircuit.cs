using FluentNHibernate.Mapping;

namespace MVCControllerTestsWithLocalDb.Web.Models
{
    public class IntegratedCircuit
    {
        public virtual int Id { get; set; }
        public virtual string Code { get; set; }
        public virtual string Description { get; set; }
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