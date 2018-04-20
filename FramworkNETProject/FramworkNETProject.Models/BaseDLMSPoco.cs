using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class BaseDLMSPoco : BasePoco
    {
        //是否有效
        [Display(Name = "是否有效", ResourceType = typeof(Resources.Language))]
        public bool IsValid { get; set; }

        //创建日期
        [Display(Name = "创建日期", ResourceType = typeof(Resources.Language))]
        public DateTime CreateDate { get; set; } 

        //创建人
        [Display(Name = "创建人", ResourceType = typeof(Resources.Language))]
        public string Creator { get; set; }

        //修改日期
        [Display(Name = "修改日期", ResourceType = typeof(Resources.Language))]
        public DateTime? ModifyDate { get; set; }
         
        //修改人
        [Display(Name = "修改人", ResourceType = typeof(Resources.Language))]
        public string Modifier { get; set; }

        //备注
        [Display(Name = "备注", ResourceType = typeof(Resources.Language))]
        public string Memo { get; set; }

    }
}
