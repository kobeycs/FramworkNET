using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Models.Attributes
{
    public class FFRequiredAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return name + Resources.Language.是必填项;
        }
    }


}
