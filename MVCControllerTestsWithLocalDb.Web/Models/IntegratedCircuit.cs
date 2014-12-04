using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCControllerTestsWithLocalDb.Web.Models
{
    public class IntegratedCircuit
    {
        public int Id { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }
    }
}