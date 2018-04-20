/* =============================================
 * 创建人：肖迪
 * 创建时间： 2011-6-14
 * 文件功能描述：Excel操作帮助类
  =============================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Utils.ExcelXML;

namespace Helpers
{
    /// <summary>
    /// Excel操作帮助类
    /// </summary>
    public class ExcelHelpers
    {
        /// <summary>
        /// 导入Excel中的数据到DataSet
        /// </summary>
        /// <param name="filePath">路径</param>
        /// <returns>DataSet</returns>
        //public static DataSet ImportExcel(string filePath,string type=null)
        //{
        //    //string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=False;IMEX=1'";
        //    string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 14.0'";

        //    using (OleDbConnection OleConn = new OleDbConnection(strConn))
        //    {
        //        OleConn.Open();
        //        string sql = "SELECT * FROM  [Sheet1$]";
        //        OleDbDataAdapter OleDaExcel = new OleDbDataAdapter(sql, OleConn);
        //        DataSet OleDsExcle = new DataSet();
        //        OleDaExcel.Fill(OleDsExcle, "Sheet1");
        //        OleConn.Close();
        //        return OleDsExcle;
        //    }
        //}
        #region 原导入，已注释
        ///// <summary>
        ///// 导入Excel数据
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public static DataSet ImportExcel(string fileName, string type)
        //{
        //    if (string.IsNullOrEmpty(type))
        //        return null;

        //    //把EXCEL导入到DataSet
        //    DataSet ds = new DataSet();
        //    string connStr = "";
        //    //if (type == "2003")
        //    //{
        //    //    connStr = " Provider = Microsoft.Jet.OLEDB.4.0; Data Source = " + fileName + ";Extended Properties=Excel 8.0;";
        //    //}
        //    //if (type == "2007")
        //    //{
        //        connStr = " Provider=Microsoft.ACE.OLEDB.12.0; Data Source = " + fileName + ";Extended Properties=Excel 12.0;";
        //    //}

        //    using (OleDbConnection conn = new OleDbConnection(connStr))
        //    {
        //        conn.Open();
        //        OleDbDataAdapter da;
        //        //for (int i = 1; i <= n; i++)
        //        //{
        //        string sql = "select * from [Sheet1$]";
        //        da = new OleDbDataAdapter(sql, conn);
        //        da.Fill(ds, "Sheet1");
        //        da.Dispose();
        //        //}
        //        conn.Close();
        //        conn.Dispose();
        //    }
        //    //删除空白行
        //    if (ds.Tables.Count > 0 && ds.Tables[0].Rows != null)
        //    {
        //        List<DataRow> NullRow = new List<DataRow>();
        //        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
        //        {
        //            bool HasValue = false;
        //            foreach (var item in ds.Tables[0].Rows[i].ItemArray)
        //            {
        //                if (item != null && item.ToString().Trim() != "")
        //                {
        //                    HasValue = true;
        //                    continue;
        //                }
        //            }
        //            if (!HasValue)
        //            {
        //                NullRow.Add(ds.Tables[0].Rows[i]);
        //            }
        //        }
        //        if (NullRow.Count > 0)
        //        {
        //            NullRow.ForEach(x => ds.Tables[0].Rows.Remove(x));
        //        }
        //    }
        //    return ds;
        //}

        #endregion

        #region 使用NPOI导入
        /// <summary>
        /// 使用NPOI导入Excel数据
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static DataSet ImportExcel(Stream stream)
        {
            DataSet ds = new DataSet();
            var wookBook = new HSSFWorkbook(stream);
            for (int sheetIndex = 0; sheetIndex < wookBook.NumberOfSheets;sheetIndex++ )
            {
                var sheet = wookBook.GetSheetAt(sheetIndex);
                var table = GenrateNPOITable(sheet.GetRow(sheet.FirstRowNum));
                for (int rowIndex = sheet.FirstRowNum + 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {

                    var tabelRow = table.NewRow();
                    var excelRow = sheet.GetRow(rowIndex);
                    if (excelRow != null)
                    {
                        for (int colIndex = 0; colIndex < table.Columns.Count; colIndex++)
                        {
                            var cell = excelRow.GetCell(colIndex);
                            if (cell != null)
                            {
                                if (cell.CellType==CellType.NUMERIC&& HSSFDateUtil.IsCellDateFormatted(cell))
                                {
                                    tabelRow[colIndex] = cell.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                                else
                                {
                                    tabelRow[colIndex] = cell.ToString();
                                }
                            }
                        }
                    }
                    table.Rows.Add(tabelRow);
                }
                //删除空行
                for (int i = table.Rows.Count-1; i >=0 ;i-- )
                {
                    var row = table.Rows[i];
                    bool IsNullRow = true;
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (!string.IsNullOrWhiteSpace(row[j].ToString()))
                        {
                            IsNullRow = false;
                            break;
                        }
                    }
                    if (IsNullRow)
                    {
                        table.Rows.Remove(row);
                    }
                }

                ds.Tables.Add(table);
            }

            return ds;
        }
        /// <summary>
        /// 获取要导入的Excel列头
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static DataTable GenrateNPOITable(IRow row)
        {
            DataTable dt = new DataTable();
            if (row != null && row.Cells.Count > 0)
            {
                for (int i = row.FirstCellNum; i <= row.LastCellNum; i++)
                {
                    var val = row.GetCell(i);
                    if (val != null && !string.IsNullOrWhiteSpace(val.ToString()))
                    {
                        dt.Columns.Add(val.ToString());
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return dt;
        }

        #endregion


        /// <summary>
        /// 由DataTable导出到Excel
        /// </summary>
        /// <param name="table">要存储的数据</param>
        /// <param name="fullPath">要存储到的服务器位置</param>
        /// <param name="columns">显示列表</param>
        public static void GetExportXML(System.Data.DataTable table, string fullPath, List<XMLColumn> columns = null, bool IsNoSplitStr=false)
        {
            Utils.ExcelXML.XMLExport xe = new Utils.ExcelXML.XMLExport();
            xe.Export(table, fullPath, columns,IsNoSplitStr);
        }

        /// <summary>
        /// add by yecs 20160811 增加根据dataset 导出excel
        /// </summary>
        /// <param name="dataset">dataset 数据集</param>
        /// <param name="fullPath">文件路径</param>
        public static void GetExportXMLByDataset(System.Data.DataSet dataset, string fullPath)
        {
            Utils.ExcelXML.XMLExport xe = new Utils.ExcelXML.XMLExport();
            xe.ExportByDataset(dataset, fullPath);
        }


        #region tanyi 2011-11-18 使用OleDB导出Excel  人员基础信息导出用到

        /// <summary>
        /// 导出到Excel（使用OleDB数据库操作方式，速度较快）
        /// </summary>
        /// <param name="sourceData">数据源</param>
        /// <param name="fullPath">导出文件全路径，包括文件名，如果使用固定模板则该文件必须存在，否则目标位置不应有同名文件。</param>
        public static void ExportToExcelByOleDB(System.Data.DataTable sourceData, string fullPath)
        {

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            //初始化
            string ExcelVersion = "8.0";
            string strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + fullPath + "';Extended Properties='Excel " + ExcelVersion + ";HDR=YES';";
            if (fullPath.EndsWith("xlsx"))
            {
                ExcelVersion = "12.0 xml";
                strConn = @"Provider=Microsoft.ACE.OleDb.12.0;Data Source='" + fullPath + "';Extended Properties='Excel " + ExcelVersion + ";HDR=YES';";
            }
            OleDbConnection oleConn = new OleDbConnection(strConn);
            try
            {
                oleConn.Open();
                OleDbCommand comm = new OleDbCommand();
                comm.CommandType = CommandType.Text;
                comm.Connection = oleConn;
                string tableName = "";

                tableName = "DataExport";
                string strCreateTable = "create table DataExport (";
                foreach (DataColumn dc in sourceData.Columns)
                {
                    strCreateTable += "[" + dc.ColumnName + "] Ntext, ";
                }
                if (strCreateTable.EndsWith(", "))
                    strCreateTable = strCreateTable.Substring(0, strCreateTable.Length - 2);
                strCreateTable += ")";
                comm.CommandText = strCreateTable;
                comm.ExecuteNonQuery();


                //拼合Sql语句，同时构建Parameter。
                string strSql = "insert into [" + tableName + "$] values (";
                for (int i = 0; i < sourceData.Columns.Count; i++)
                {
                    strSql += "@p" + i.ToString() + ",";
                    OleDbParameter p = new OleDbParameter();
                    p.ParameterName = "@p" + i.ToString();
                    p.DbType = ConvertType(sourceData.Columns[i].DataType);
                    comm.Parameters.Add(p);
                }
                strSql = strSql.Substring(0, strSql.Length - 1) + ")";

                comm.CommandText = strSql;
                comm.Connection = oleConn;

                int perSheetRowCount = fullPath.EndsWith("xlsx") ? 1048575 : 65535;
                int sheetRowCount = perSheetRowCount;
                for (int i = 1; i <= sourceData.Rows.Count; i++)
                {
                    if (i > sheetRowCount)
                    {
                        comm.CommandText = "select * into [" + tableName + ((int)(i / perSheetRowCount)).ToString() + "] from [" + tableName + "$] where 1 = 2 ";
                        comm.ExecuteNonQuery();

                        comm.CommandText = strSql.Replace("[" + tableName + "$]", "[" + tableName + ((int)(i / perSheetRowCount)).ToString() + "$]");

                        sheetRowCount += perSheetRowCount;
                    }

                    DataRow dr = sourceData.Rows[i - 1];

                    //逐个读取每行的每个字段，填入Parameter
                    for (int j = 0; j < dr.ItemArray.Length; j++)
                    {
                        comm.Parameters[j].Value = dr[j];
                    }
                    //执行并插入Excel
                    comm.ExecuteNonQuery();
                }

            }
            finally
            {
                oleConn.Close();
            }
        }

        private static DbType ConvertType(Type t)
        {
            switch (t.ToString())
            {
                case "System.String":
                    return DbType.String;

                case "System.UInt64":
                    return DbType.UInt64;

                case "System.Int64":
                    return DbType.Int64;

                case "System.Int32":
                    return DbType.Int32;

                case "System.UInt32":
                    return DbType.UInt32;

                case "System.Single":
                    return DbType.Single;

                case "System.DateTime":
                    return DbType.DateTime;

                case "System.UInt16":
                    return DbType.UInt16;

                case "System.Int16":
                    return DbType.Int16;

                case "System.Byte":
                    return DbType.SByte;

                case "System.Decimal":
                    return DbType.Decimal;

                case "System.Double":
                    return DbType.Double;

                case "System.Byte[]":
                    return DbType.Binary;

                case "System.Guid":
                    return DbType.Guid;

                case "System.Boolean":
                    return DbType.Boolean;
            }
            return DbType.String;
        }


        /// <summary>
        /// 获取GetDataSet
        /// </summary>
        /// <param name="cmdType"></param>
        /// <param name="cmdText"></param>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandTimeout = 600;
            SqlConnection conn = new SqlConnection(GetConfigString("Release"));

            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);

                using (SqlDataAdapter myAdapter = new SqlDataAdapter(cmd))
                {
                    DataSet myDataSet = new DataSet();
                    myAdapter.Fill(myDataSet);
                    cmd.Parameters.Clear();

                    return myDataSet;
                }
            }
            catch (Exception ex)
            {
                conn.Close();

                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }


        /// <summary>
        /// 得到AppSettings中的配置string信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfigString(string key)
        {
            string result = string.Empty;
            string cfgVal = System.Configuration.ConfigurationManager.ConnectionStrings[key].ConnectionString;
            if (null != cfgVal && string.Empty != cfgVal)
            {
                try
                {
                    result = cfgVal;
                }
                catch (FormatException)
                {
                    // Ignore format exceptions.
                }
            }

            return result;
        }


        /// <summary>
        /// 初始化一个SqlCommand对象
        /// </summary>
        /// <param name="command">需要初始化的SqlCommand对象</param>
        /// <param name="connection">一个SQL连接对象</param>
        /// <param name="transaction">一个事务对象</param>
        /// <param name="commandtype">SQL命令的类型，COMMANDTYPE枚举类型</param>
        /// <param name="commandText">SQL命令</param>
        /// <param name="commandParameter">参数集合数组</param>
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandtype, string commandText, SqlParameter[] commandParameter)
        {
            try
            {
                //判断连接状态,如果未打开连接，则打开
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                //设置COMMAND的连接属性
                command.Connection = connection;

                //设置COMMANDTEXT属性
                command.CommandText = commandText;

                //设置COMMANDtype属性
                command.CommandType = commandtype;

                //如果事务对象可用，则设置COMMAND的事务属性
                if (transaction != null)
                {
                    command.Transaction = transaction;
                }

                //设置COMMAND的参数集合
                if (commandParameter != null)
                {
                    AttachParameters(command, commandParameter);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return;
        }

        /// <summary>
        /// 该方法用于将 SqlParameters[] 参数数组绑定到一个 SqlCommand 上。
        ///
        /// 该方法将为任何一个方向为 InputOutput 输入/输出且值为 null 的参数赋一个 DBNull.Value 值。
        ///
        /// 该行为将阻止使用默认值，但是这对于一个有意设置的纯粹的 output 输出参数（源自 InputOutput），
        /// 而用户并未提供输入值得情况并不太通用。
        /// </summary>
        /// <param name="command">即将添加参数的 command 命令</param>
        /// <param name="commandParameters">一个被添加到 command 命令的 SqlParameter[] 参数数组</param>
        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (SqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // 检查输入的 output 输出值是否被赋值
                        if ((p.Direction == ParameterDirection.InputOutput ||
                            p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }


        #endregion


    }
}