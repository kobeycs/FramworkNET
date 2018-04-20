using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using ViewModels;

namespace DLMS.ViewModels
{
    public static class BaseVMExt
    {
        /// <summary>
        /// 根据部门ID获取处1
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Subsection GetSubsection(this BaseVM self, long id)
        {
            Subsection Subsection = self.DC.RunSP<Subsection>("system_get_getsubsection @pid,@lcode",
                new SqlParameter { ParameterName = "pid ", Value = id },
                 new SqlParameter { ParameterName = "lcode ", Value = self.CurrentLanguage.LanguageCode }
                ).FirstOrDefault();
            return Subsection;
        }
        /// <summary>
        /// 根据部门ID获取子部门ID列表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<long> GetChildrenDeparement(this BaseVM self, long id)
        {
            List<long> depids = self.DC.RunSP<long>("system_get_getchildrendeparement @pid", new SqlParameter { ParameterName = "pid ", Value = id }).ToList();
            return depids;
        }
        
    }

    public class Subsection
    {
        public long ID { get; set; }
        public long? ParentDepartmentID { get; set; }
        public bool IsSubsection { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
    }

    public class AutoCompleteData
    {
        /// <summary>
        /// ID
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// 文本
        /// </summary>
        public string label { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string code { get; set; }
    }
    public class DeptID
    {
        /// <summary>
        /// 部门ID
        /// </summary>
        public long? id { get; set; }
        /// <summary>
        /// 0：下级部门，1：上级部门
        /// </summary>
        public long hasRight { get; set; }
    }
    #region  优化ajax获取用户的控件 add by samsundot at 20151109
    public class UserList
    {
        /// <summary>
        /// ID
        /// </summary>
        public long? id { get; set; }
        /// <summary>
        /// 中文名
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public string UserCode { get; set; }
    }
    #endregion 

    public class TreeData
    {
        public TreeData()
        {
            //默认全部展开
            expand = true;
        }
        /// <summary>
        /// 节点显示文字
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 节点Key（ID）
        /// </summary>
        public long key { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 可用
        /// </summary>
        public bool enabled { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>
        public bool expand { get; set; }
        /// <summary>
        /// 是否为处
        /// </summary>
        public bool IsSubsection { get; set; }
        /// <summary>
        /// 子节点
        /// </summary>
        public List<TreeData> children { get; set; }
    }

}