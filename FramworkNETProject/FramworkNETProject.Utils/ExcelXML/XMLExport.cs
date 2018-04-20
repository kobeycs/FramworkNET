using System;
using System.Text;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Utils.ExcelXML
{
    public class XMLExport
    {
        /// <summary>
        /// 解决GetXMLString方法里拼接stringBuilder报内存溢出的问题（原因不详，只是偶尔出现，重启服务器就好了） 2015-07-03
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filePath"></param>
        /// <param name="columns"></param>
        /// <param name="IsNoSplitStr">新加参数，确保不影响别的逻辑</param>
        public void Export(DataTable table, string filePath, List<XMLColumn> columns = null, bool IsNoSplitStr = true)
        {
            if (IsNoSplitStr)//一次拼接好写入文件流,可能会导致内存溢出问题
            {
                var xmlstr = GetXMLString(table, columns);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.Write(xmlstr);
                        writer.Flush();
                        writer.Close();

                    }
                    fs.Close();
                }
                GC.Collect();
            }
            else
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        var header = new XMLHeader();
                        if (columns != null)
                        {
                            header.Columns = columns;
                        }
                        else
                        {
                            header.GenrateColumns(table.Columns);
                        }

                        StringBuilder buidStr = new StringBuilder(XMLValue.Header);
                        //获取标题
                        buidStr.Append("<Row>");
                        foreach (var col in header.Columns)
                        {
                            buidStr.Append("<Cell ss:StyleID='sh'><Data ss:Type='String'>" + col.ColumnName + "</Data></Cell>");
                        }
                        buidStr.Append("</Row>");

                        writer.Write(buidStr.ToString());
                        buidStr.Clear();

                        //获取内容
                        foreach (DataRow row in table.Rows)
                        {
                            buidStr.Append("<Row ss:AutoFitHeight='0'>");
                            foreach (var col in header.Columns)
                            {
                                if (col.Format != null)
                                {

                                    var val = col.Format.Invoke(row);
                                    var value = val == null ? "" : val.ToString();
                                    buidStr.Append(GetCellString(col.ColumnType, value));
                                }
                                else
                                {
                                    buidStr.Append(GetCellString(col.ColumnType, row[col.DataName].ToString()));
                                }
                            }

                            buidStr.Append("</Row>");
                            writer.Write(buidStr.ToString());
                            buidStr.Clear();
                        }

                        buidStr.Append(XMLValue.Bottom);
                        writer.Write(buidStr.ToString());
                        writer.Flush();
                        writer.Close();

                    }
                    fs.Close();
                }

            }
        }



        public void ExportByDataset(DataSet dataset, string filePath)
        {
            StringBuilder buidStr = new StringBuilder();
            buidStr.Append(XMLMoreSheetValue.Header);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    if (dataset != null && dataset.Tables.Count > 0)
                    {
                        foreach (DataTable table in dataset.Tables)
                        {
                            XMLMoreSheetValue.sheetName = table.TableName;

                            buidStr.Append(string.Format("<Worksheet ss:Name='{0}'><Table>", XMLMoreSheetValue.sheetName));

                            var header = new XMLHeader();

                            header.GenrateColumns(table.Columns);


                            //获取标题
                            buidStr.Append("<Row>");
                            foreach (var col in header.Columns)
                            {
                                buidStr.Append("<Cell ss:StyleID='sh'><Data ss:Type='String'>" + col.ColumnName + "</Data></Cell>");
                            }
                            buidStr.Append("</Row>");

                            writer.Write(buidStr.ToString());
                            buidStr.Clear();

                            //获取内容
                            foreach (DataRow row in table.Rows)
                            {
                                buidStr.Append("<Row ss:AutoFitHeight='0'>");
                                foreach (var col in header.Columns)
                                {
                                    if (col.Format != null)
                                    {

                                        var val = col.Format.Invoke(row);
                                        var value = val == null ? "" : val.ToString();
                                        buidStr.Append(GetCellString(col.ColumnType, value));
                                    }
                                    else
                                    {
                                        buidStr.Append(GetCellString(col.ColumnType, row[col.DataName].ToString()));
                                    }
                                }

                                buidStr.Append("</Row>");
                                
                            }

                            buidStr.Append("</Table></Worksheet>");
                            writer.Write(buidStr.ToString());
                            buidStr.Clear();
                        }
                    }
                    buidStr.Append(XMLMoreSheetValue.Bottom);
                    writer.Write(buidStr.ToString());
                    writer.Flush();
                    writer.Close();


                }
                fs.Close();
            }
        }

        /// <summary>
        /// 生成XML字符串
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="columns">显示列，不输入则使用datatable的列</param>
        /// <returns></returns>
        private string GetXMLString(DataTable table, List<XMLColumn> columns = null)
        {
            var header = new XMLHeader();
            if (columns != null)
            {
                header.Columns = columns;
            }
            else
            {
                header.GenrateColumns(table.Columns);
            }

            StringBuilder buidStr = new StringBuilder(XMLValue.Header);
            //获取标题
            buidStr.Append("<Row>");
            foreach (var col in header.Columns)
            {
                buidStr.Append("<Cell ss:StyleID='sh'><Data ss:Type='String'>" + col.ColumnName + "</Data></Cell>");
            }
            buidStr.Append("</Row>");

            //获取内容
            foreach (DataRow row in table.Rows)
            {
                buidStr.Append("<Row ss:AutoFitHeight='0'>");
                foreach (var col in header.Columns)
                {
                    if (col.Format != null)
                    {

                        var val = col.Format.Invoke(row);
                        var value = val == null ? "" : val.ToString();
                        buidStr.Append(GetCellString(col.ColumnType, value));
                    }
                    else
                    {
                        buidStr.Append(GetCellString(col.ColumnType, row[col.DataName].ToString()));
                    }
                }
                buidStr.Append("</Row>");
            }
            buidStr.Append(XMLValue.Bottom);
            return buidStr.ToString();
        }

        /// <summary>
        /// 行单元格
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetCellString(XMLColumnType type, string value)
        {

            string cellValue = value;
            string cellStyle = "";
            decimal decimalvalue;
            if (type == XMLColumnType.DateTime)
            {
                DateTime dt;
                if (DateTime.TryParse(value, out dt))
                {
                    cellValue = dt.ToString("yyyy-MM-ddTHH:mm:ss");
                    //cellValue = dt.ToString("yyyy-MM-dd");
                }
                cellStyle = " ss:StyleID='sd'";

                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    type = XMLColumnType.String;
                    cellStyle = "";
                }
            }
            //如果列的类型是数字类型，且数据能正常转换成decimal类型 ,add by yecs 20160712
            else if (type == XMLColumnType.Number && decimal.TryParse(value, out decimalvalue))
            {
                cellStyle = " ss:StyleID='sc'";
                type = XMLColumnType.Number;
            }
            else
            {
                Regex reg = new Regex(@"^[-]?\d+[.]?\d*$");
                if (value != null && reg.IsMatch(value))
                {
                    if (value.Contains(".") || value == "0" || (value.Length < 6 && !value.StartsWith("0")) || value.StartsWith("-"))
                    {
                        type = XMLColumnType.Number;

                    }
                    else
                    {
                        type = XMLColumnType.String;
                    }
                }
                else
                {
                    type = XMLColumnType.String;
                }
                cellStyle = " ss:StyleID='sc'";
            }

            return "<Cell" + cellStyle + "><Data ss:Type='" + type.ToString() + "'>" + cellValue + "</Data></Cell>";

        }
    }
}
