using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.System
{
    [Table("System_ActionLogMLContent")]
    public class ActionLogMLContent : MLContent<ActionLog>
    {
        [Display(Name = "ģ��", ResourceType = typeof(Resources.Language))]
        public string ModelName { get; set; }
        [Display(Name = "����", ResourceType = typeof(Resources.Language))]
        public string ActionName { get; set; }

    }
}
