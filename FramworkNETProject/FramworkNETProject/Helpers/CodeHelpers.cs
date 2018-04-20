using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using DataAccess.SqlServer;

namespace Helpers
{
    public static class CodeHelpers
    {
        #region 生成加班单据号
        /// <summary>
        /// 生成加班单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateOvertimeCode()
        {

            return "OVE-" + DateTime.Now.ToString("yyyyMMdd") + "-" + GetSeed("Overtime");
        }
        #endregion

        #region 生成旷工单据号
        /// <summary>
        /// 生成旷工单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateAbsenteeismCode()
        {

            return "ABS-" + DateTime.Now.ToString("yyyyMMdd") + "-" + GetSeed("Absenteeism");
        }
        #endregion

        #region 生成补卡单据号
        /// <summary>
        /// 生成补卡单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateRepairCardCode()
        {

            return "REP-" + DateTime.Now.ToString("yyyyMMdd") + "-" + GetSeed("RepairCard");
        }
        #endregion

        #region 生成休假单据号
        /// <summary>
        /// 生成休假单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateVacationCode()
        {

            return "VAC-" + DateTime.Now.ToString("yyyyMMdd") + "-" + GetSeed("Vacation");
        }
        #endregion

        #region 生成招聘单据号
        /// <summary>
        /// 生成招聘单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateRecruitmentCode()
        {

            return "REC" + DateTime.Now.ToString("yyyyMMdd") + GetSeed("Recruitment");
        }
        #endregion

        #region 生成离职单据号
        /// <summary>
        /// 生成离职单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateDismissionCode()
        {

            return "DIM" + DateTime.Now.ToString("yyyyMMdd") + GetSeed("Dismission");
        }
        #endregion

        #region 生成异动单据号
        /// <summary>
        /// 生成异动单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateTransactionCode()
        {

            return "TRN" + DateTime.Now.ToString("yyyyMMdd") + GetSeed("Transaction");
        }
        #endregion

        #region 生成岗位认证单据号
        /// <summary>
        /// 生成岗位认证单据号
        /// </summary>
        /// <returns></returns>
        public static string GenerateStationAttestationCode()
        {

            return "JOB-" + DateTime.Now.ToString("yyyyMMdd") + "-" + GetSeed("StationAttestation");
        }
        #endregion

        #region 生成工薪项导入单据号
        /// <summary>
        /// 生成工薪项导入单据号
        /// </summary>
        /// <returns></returns>
        public static string GeneratePayrollNewInputCode()
        {

            return "PAY-" + DateTime.Now.ToString("yyyyMMdd") + "-" + GetSeed("PayrollNewInput");
        }
        #endregion

        #region 生成种子
        /// <summary>
        /// 生成种子         
        /// </summary>
        /// <param name="moudle">模块名称</param>
        /// <param name="length">流水号长度,如：4则最后返回的结果为0001-9999,重置频率为1天</param>
        /// <returns></returns>
        public static string GetSeed(string moudle, int length = 4)
        {
            using (var dc = new DataContext())
            {
                string code = "";
                if (!string.IsNullOrEmpty(moudle))
                {
                    SqlParameter paramOut = new SqlParameter("@no", SqlDbType.NVarChar, length);
                    paramOut.Direction = ParameterDirection.Output;
                    var sqls = new SqlParameter[] { new SqlParameter("@Moudle", moudle), new SqlParameter("@Length", length), paramOut };
                    dc.RunSP("proc_get_seed", sqls);
                    if (paramOut.Value != null)
                    {
                        code = paramOut.Value.ToString();
                    }
                }
                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception(Resources.Language.无法生成编号种子);
                }
                return code;
            }

        }

        #endregion



    }
}