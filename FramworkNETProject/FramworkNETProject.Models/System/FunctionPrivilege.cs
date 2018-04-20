using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.System
{
    [Table("System_FunctionPrivilege")]
    public class FunctionPrivilege : BasePoco
    {
        [Required(AllowEmptyStrings=false)]
        public string RoleName { get; set; }
        public long PageFunctionID { get; set; }
        public PageFunction PageFunction { get; set; }
    }
}
