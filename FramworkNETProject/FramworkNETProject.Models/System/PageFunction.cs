using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.System
{
    [Table("System_PageFunction")]
    public class PageFunction : BasePoco, IMLData<PageFunctionMLContent>, ITreeData<PageFunction>
    {
        [Display(Name="Url")]
        public string Url { get; set; }
        [Display(Name = "≤Àµ•œ‘ æ", ResourceType = typeof(Resources.Language))]
        public bool IsShownOnMenu { get; set; }
        public long? ParentID { get; set; }
        public PageFunction Parent { get; set; }
        public List<PageFunction> Children { get; set; }
        public List<PageFunctionMLContent> MLContents { get; set; }
        public List<FunctionPrivilege> Privileges { get; set; }
    }
}
