using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using amorphie.template.core.Model;

namespace amorphie.template.Validator;
    public sealed class DepositMobApprovalValidator : AbstractValidator<DepositMobApproval>
    {
        public DepositMobApprovalValidator()
        {
            RuleFor(x => x.Iban).NotNull();
            RuleFor(x => x.FullName).NotNull();
        }
    }

public static class HealthCheckModule 
{
 
}

