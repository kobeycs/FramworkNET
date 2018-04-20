using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Models.Validators
{
    public class CommonValidator
    {
        public static ValidationResult DateShouldLessThanToday(DateTime? dt, ValidationContext context)
        {

            if (dt == null)
            {
                return ValidationResult.Success;
            }
            if (dt.Value.Date < DateTime.Today)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(null, new string[] { context.MemberName });
            }
        }
    }
}