using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
namespace SupportClasses
{
    public class OCFUser
    {
        [Display(Name="GUID")]
        public string GUID { get; set; }
        [Display(Name = "ITCode")]
        public string ITCode { get; set; }
        [Display(Name = "中文名", ResourceType = typeof(Resources.Language))]
        public string ChineseName { get; set; }
        [Display(Name = "英文名", ResourceType = typeof(Resources.Language))]
        public string EnglishName { get; set; }
        [Display(Name = "部门", ResourceType = typeof(Resources.Language))]
        public string Department { get; set; }
        [Display(Name = "电话", ResourceType = typeof(Resources.Language))]
        public string Tel { get; set; }
        [Display(Name = "电子邮件", ResourceType = typeof(Resources.Language))]
        public string Email { get; set; }
        
        /// <summary>
        /// 角色列表
        /// </summary>
        public List<string> Roles { get; set; }
        
        public bool IsInRole(string role)
        {
            if (Roles.Contains(role))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
