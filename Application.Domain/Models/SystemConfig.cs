using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Domain.Models
{
    public class SystemConfig
    {
        public string P1Equation { get; set; } = null!;
        public string P2Equation { get; set; } = null!;
        public string P3Equation { get; set; } = null!;
        public string XPEquation { get; set; } = null!;

        public double ProjectDuration { get; set; }
        public double VoucherExpireDuration { get; set; }
        public string MaxMemberSendPerMonthEquation { get; set; } = null!;
    }
}