using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.core.Base;

namespace amorphie.template.core.Model;

public class DepositMobApproval : EntityBase
{
    public string Iban { get; set; }
    public long CitizenshipNumber { get; set; }
    public string FullName { get; set; }
    public DateTime MobApprovalDate { get; set; }
    public bool IsMobApproved { get; set; }
}
