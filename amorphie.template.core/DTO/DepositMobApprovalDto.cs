using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using amorphie.core.Base;

namespace amorphie.template.core.DTO
{
    public class DepositMobApprovalDto : DtoBase
    {
        public string Iban { get; set; }
        public string FullName { get; set; }
    }
}