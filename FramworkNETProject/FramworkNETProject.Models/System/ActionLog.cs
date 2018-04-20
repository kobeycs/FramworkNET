using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.System
{
    public enum ActionLogTypes { 正常, 异常 };
    [Table("System_ActionLog")]
    public class ActionLog : BasePoco,IMLData<ActionLogMLContent>
    {
        [Display(Name = "ITCode")]
        public string ITCode { get; set; }
        [Display(Name = "Url")]
        public string ActionUrl { get; set; }
        [Display(Name = "操作时间", ResourceType = typeof(Resources.Language))]
        public DateTime ActionTime { get; set; }
        [Display(Name="耗时", ResourceType=typeof(Resources.Language))]
        public double Duration { get; set; }
        [StringLength(int.MaxValue)]
        [Display(Name = "备注", ResourceType = typeof(Resources.Language))]
        public string Remark { get; set; }
        [Display(Name = "操作", ResourceType = typeof(Resources.Language))]
        public string LogType { get; set; }
        public List<ActionLogMLContent> MLContents { get; set; }
    }
}
