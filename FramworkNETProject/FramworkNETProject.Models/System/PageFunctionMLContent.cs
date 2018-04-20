using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Models.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.System
{
    [Table("System_PageFunctionMLContent")]
    public class PageFunctionMLContent : MLContent<PageFunction>
    {
        [Display(Name = "Ò³ÃæÃû³Æ", ResourceType = typeof(Resources.Language))]
        [MLRequiredAttribute(AllLanguageRequired=false)]
        public string FunctionName { get; set; }
    }
}
