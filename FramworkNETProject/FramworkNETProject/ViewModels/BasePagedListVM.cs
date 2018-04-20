using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using org.in2bits.MyXls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using NPOI.XSSF.UserModel;
using Models;
using NPOI.HSSF.UserModel;
using Helpers;
using SupportClasses;

namespace ViewModels
{
    public class BasePagedListVM<TModel, TSearcher> : BaseVM, IValidatableObject, IPagedList<TModel, TSearcher>
        where TModel : class
        where TSearcher : BaseSearcher
    {
        [XmlIgnore]
        public List<TModel> EntityList { get; set; }
        public TSearcher Searcher { get; set; }
        [XmlIgnore]
        public List<IGridColumn<TModel>> ListColumns { get; set; }

        public int RecordsPerPage { get; set; }
        public bool NeedPage { get; set; }
        public bool ShowHeader { get; set; }

        public BasePagedListVM()
        {
            NeedPage = true;
            ShowHeader = true;
            EntityList = new List<TModel>();
            Searcher = typeof(TSearcher).GetConstructor(Type.EmptyTypes).Invoke(null) as TSearcher;
            string Rpp = System.Configuration.ConfigurationManager.AppSettings["RPP"];
            int tempRPP;
            if (!int.TryParse(Rpp, out tempRPP))
            {
                RecordsPerPage = 15;
            }
            else
            {
                RecordsPerPage = tempRPP;
            }
        }

        public virtual List<IGridColumn<TModel>> SetDisplayColumns()
        {
            return new List<IGridColumn<TModel>>();
        }

        public virtual IOrderedQueryable<TModel> GetSearchQuery()
        {
            if (typeof(TModel).IsSubclassOf(typeof(BasePoco)))
            {
                return DC.GetSet<TModel>().OrderBy(x => (x as BasePoco).ID);
            }
            else
            {
                return null;
            }
        }

        public void DisplayHeader()
        {
            ListColumns = SetDisplayColumns();
        }

        public void DoSearch()
        {
            var query = GetSearchQuery();
            if (ShowHeader)
            {
                ListColumns = SetDisplayColumns();
            }
            int count = query.Count() - 1;
            if (count < 0)
            {
                count = 0;
            }
            Searcher.TotalPages = count / RecordsPerPage + 1;
            if (Searcher.CurrentPage >= Searcher.TotalPages)
            {
                Searcher.CurrentPage = Searcher.TotalPages - 1;
            }
            //更改，郝志强
            if (Searcher.CurrentPage < 0)
            {
                Searcher.CurrentPage = 0;
            }

            //更改结束

            if (NeedPage)
            {
                EntityList = query.Skip(Searcher.CurrentPage * RecordsPerPage).Take(RecordsPerPage).ToList();
            }
            else
            {
                EntityList = query.ToList();
                Searcher.CurrentPage = -1;
            }
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> rv = new List<ValidationResult>();
            return rv;
        }

        #region MyXls导出
        /// <summary>
        /// MyXls导出
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateExcel()
        {
            XlsDocument xls = new XlsDocument();

            Worksheet sheet = xls.Workbook.Worksheets.Add("Sheet1");//状态栏标题名称
            Cells cells = sheet.Cells;
            MakeExcelHeader(ListColumns, cells, 1, 1);

            int rowIndex = ListColumns.Max(x => x.MaxLevel);
            int colIndex = 0;
            //List<Color> allcolors = new List<Color>();
            //allcolors.Add(Colors.Default08);
            //allcolors.Add(Colors.Default09);
            //allcolors.Add(Colors.Default0A);
            //allcolors.Add(Colors.Default0B);
            //allcolors.Add(Colors.Default0C);
            //allcolors.Add(Colors.Default0D);
            //allcolors.Add(Colors.Default0E);
            //allcolors.Add(Colors.Default0F);
            //allcolors.Add(Colors.Default10);
            //allcolors.Add(Colors.Default11);
            //allcolors.Add(Colors.Default12);
            //allcolors.Add(Colors.Default13);
            //allcolors.Add(Colors.Default14);
            //allcolors.Add(Colors.Default15);
            //allcolors.Add(Colors.Default16);
            //allcolors.Add(Colors.Default17);
            //allcolors.Add(Colors.Default18);
            //allcolors.Add(Colors.Default19);
            //allcolors.Add(Colors.Default20);
            //allcolors.Add(Colors.Default1A);
            //allcolors.Add(Colors.Default1B);
            //allcolors.Add(Colors.Default1C);
            //allcolors.Add(Colors.Default1D);
            //allcolors.Add(Colors.Default1E);
            //allcolors.Add(Colors.Default1F);
            //allcolors.Add(Colors.Default20);
            //allcolors.Add(Colors.Default21);
            //allcolors.Add(Colors.Default22);
            //allcolors.Add(Colors.Default23);
            //allcolors.Add(Colors.Default24);
            //allcolors.Add(Colors.Default25);
            //allcolors.Add(Colors.Default26);
            //allcolors.Add(Colors.Default27);
            //allcolors.Add(Colors.Default28);
            //allcolors.Add(Colors.Default29);
            //allcolors.Add(Colors.Default2A);
            //allcolors.Add(Colors.Default2B);
            //allcolors.Add(Colors.Default2C);
            //allcolors.Add(Colors.Default2D);
            //allcolors.Add(Colors.Default2E);
            //allcolors.Add(Colors.Default2F);
            //allcolors.Add(Colors.Default30);
            //allcolors.Add(Colors.Default31);
            //allcolors.Add(Colors.Default32);
            //allcolors.Add(Colors.Default33);
            //allcolors.Add(Colors.Default34);
            //allcolors.Add(Colors.Default35);
            //allcolors.Add(Colors.Default36);
            //allcolors.Add(Colors.Default37);
            //allcolors.Add(Colors.Default38);
            //allcolors.Add(Colors.Default39);
            //allcolors.Add(Colors.Default3A);
            //allcolors.Add(Colors.Default3B);
            //allcolors.Add(Colors.Default3C);
            //allcolors.Add(Colors.Default3D);
            //allcolors.Add(Colors.Default3E);
            //allcolors.Add(Colors.Default3F);
            //int xx = 0;
            Dictionary<IGridColumn<TModel>, int> sameCount = new Dictionary<IGridColumn<TModel>, int>();
            foreach (var baseCol in ListColumns)
            {
                foreach (var col in baseCol.BottomChildren)
                {
                    sameCount.Add(col, 0);
                }
            }
            int i = 0;
            foreach (var row in EntityList)
            {
                rowIndex++;
                colIndex = 0;
                foreach (var baseCol in ListColumns)
                {
                    foreach (var col in baseCol.BottomChildren)
                    {
                        colIndex++;
                        //sheet.Cells.AddValueCell(rowIndex, colIndex, row[col.ColumnName].ToString());//将数据添加到xls表格里
                        //Cell cell= cells.AddValueCell(rowIndex, colIndex, Convert.ToDouble(row[col.ColumnName].ToString()));//转换为数字型
                        string cellValue = col.GetText(row).ToHtmlString();
                        object cv = cellValue;
                        int testInt;
                        decimal testFloat;
                        int floatRound = 0;
                        if (!cellValue.StartsWith("0"))
                        {
                            if (int.TryParse(cellValue, out testInt))
                            {
                                if (testInt <= 100000)
                                {
                                    cv = testInt;
                                }
                            }
                            else if (decimal.TryParse(cellValue, out testFloat))
                            {
                                if (testFloat <= 100000)
                                {
                                    cv = testFloat;
                                    if (cellValue.Split('.').Count() > 1)
                                    {
                                        floatRound = cellValue.Split('.')[1].ToArray().Count(); //获取小数位数
                                    }
                                }
                            }
                        }
                        else if (decimal.TryParse(cellValue, out testFloat))
                        {
                            if (testFloat == 0)//为0时
                            {
                                cv = 0;
                                floatRound = 0;
                            }
                            else if (testFloat < 1)//modify by yecs 2015-02-26 LSTC税收报表: 小数点的项目被显示为文本格式（小于1的小数）
                            {
                                cv = testFloat;
                                floatRound = cellValue.Split('.')[1].ToArray().Count();
                            }
                        }
                        Cell cell = cells.Add(rowIndex, colIndex, cv);
                        if (floatRound > 0)
                        {
                            cell.Format = "0.";
                            for(int x=1;x<=floatRound;x++)
                            {
                                cell.Format += "0";
                            }
                            
                        }
                        cell.TopLineStyle = 1;
                        cell.BottomLineStyle = 1;
                        cell.LeftLineStyle = 1;
                        cell.RightLineStyle = 1;
                        cell.UseBorder = true;
                        cell.VerticalAlignment = VerticalAlignments.Centered;
                        System.Drawing.Color backColor = col.GetBackGroundColor(row);
                        System.Drawing.Color foreColor = col.GetForeGroundColor(row);
                        if (backColor != System.Drawing.Color.Empty)
                        {
                            cell.Pattern = 1;
                            cell.PatternColor = UtilsTool.GetExcelColor(backColor);
                        }

                        if (col.NeedGroup == true && sameCount[col] == i)
                        {
                            sameCount[col] = sameCount[col] + 1;
                            string lastValue = col.GetText(row).ToHtmlString();
                            for (int j = i + 1; j < EntityList.Count; j++)
                            {
                                if (col.GetText(EntityList[j]).ToHtmlString() == lastValue)
                                {
                                    sameCount[col] = sameCount[col] + 1;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            cells.Merge(rowIndex, rowIndex + sameCount[col] - i - 1, colIndex, colIndex);
                        }
                    }
                }
                i++;
            }

            return xls.Bytes.ByteArray;
        }

        private void MakeExcelHeader(List<IGridColumn<TModel>> cols, Cells cells, int colIndex, int rowIndex)
        {
            int maxLevel = cols.Max(x => x.MaxLevel);
            foreach (var col in cols)
            {
                //sheet.Cells.AddValueCell(1,colIndex,col.ColumnName);//添加XLS标题行

                int maxRow = maxLevel - col.MaxLevel + rowIndex;
                int maxCol = colIndex + col.MaxChildrenCount - 1;
                cells.Merge(rowIndex, maxRow, colIndex, maxCol);
                Cell cell = cells.Add(rowIndex, colIndex, col.Header);
                cell.Font.Weight = FontWeight.Bold;
                cell.TopLineStyle = 1;
                cell.BottomLineStyle = 1;
                cell.LeftLineStyle = 1;
                cell.RightLineStyle = 1;
                cell.UseBorder = true;
                cell.VerticalAlignment = VerticalAlignments.Centered;
                cell.HorizontalAlignment = HorizontalAlignments.Centered;
                for (int r = rowIndex + 1; r <= maxRow; r++)
                {
                    Cell cell1 = cells.Add(r, colIndex, col.Header);
                    cell1.Font.Weight = FontWeight.Bold;
                    cell1.TopLineStyle = 1;
                    cell1.BottomLineStyle = 1;
                    cell1.LeftLineStyle = 1;
                    cell1.RightLineStyle = 1;
                    cell1.UseBorder = true;
                    cell1.VerticalAlignment = VerticalAlignments.Centered;
                    cell1.HorizontalAlignment = HorizontalAlignments.Centered;
                }
                for (int c = colIndex + 1; c <= maxCol; c++)
                {
                    Cell cell1 = cells.Add(rowIndex, c, col.Header);
                    cell1.Font.Weight = FontWeight.Bold;
                    cell1.TopLineStyle = 1;
                    cell1.BottomLineStyle = 1;
                    cell1.LeftLineStyle = 1;
                    cell1.RightLineStyle = 1;
                    cell1.UseBorder = true;
                    cell1.VerticalAlignment = VerticalAlignments.Centered;
                    cell1.HorizontalAlignment = HorizontalAlignments.Centered;
                }

                if (col.Children != null && col.Children.Count > 0)
                {
                    MakeExcelHeader(col.Children, cells, colIndex, rowIndex + 1);
                }
                colIndex = maxCol + 1;
            }
        }
        #endregion

        #region NPOI导出
        /// <summary>
        /// NPOI导出
        /// <remarks>通过EntityList导出</remarks>
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateExcelByNPOI()
        {
            //数字
            Regex reg = new Regex(@"^[-]?\d+[.]?\d*$");

            int sheetno = ((EntityList.Count - 1) / 60000) + 1;
            HSSFWorkbook workbook = new HSSFWorkbook();
            List<HSSFSheet> sheets = new List<NPOI.HSSF.UserModel.HSSFSheet>();
            for (int i = 1; i <= sheetno; i++)
            {
                HSSFSheet sheet = workbook.CreateSheet("Sheet" + i) as HSSFSheet;
                HSSFRow dataRow = sheet.CreateRow(0) as HSSFRow;
                MakeExcelHeader(ListColumns, dataRow, 0, 0);
                sheets.Add(sheet);
            }


            int rowIndex = 0;
            int colIndex = 0;
            Dictionary<IGridColumn<TModel>, int> sameCount = new Dictionary<IGridColumn<TModel>, int>();
            foreach (var baseCol in ListColumns)
            {
                foreach (var col in baseCol.BottomChildren)
                {
                    sameCount.Add(col, 0);
                }
            }
            foreach (var row in EntityList)
            {
                rowIndex++;
                int sheetindex = ((rowIndex - 1) / 60000);
                colIndex = 0;
                System.Drawing.Color bgColor = System.Drawing.Color.Empty;
                System.Drawing.Color fColor = System.Drawing.Color.Empty;
                HSSFRow dr = sheets[sheetindex].CreateRow(rowIndex - sheetindex * 60000) as HSSFRow;

                foreach (var baseCol in ListColumns)
                {
                    foreach (var col in baseCol.BottomChildren)
                    {
                        //sheet.Cells.AddValueCell(rowIndex, colIndex, row[col.ColumnName].ToString());//将数据添加到xls表格里
                        //Cell cell= cells.AddValueCell(rowIndex, colIndex, Convert.ToDouble(row[col.ColumnName].ToString()));//转换为数字型
                        string text = Regex.Replace(col.GetText(row).ToHtmlString(), @"<[^>]*>", String.Empty);
                        var c = dr.CreateCell(colIndex);



                        //如果为数字的话
                        if (text != null && reg.IsMatch(text))
                        {
                            if (text.Contains('.') || text == "0" || (text.Length < 6 && !text.StartsWith("0")))
                            {
                                c.SetCellValue(double.Parse(text));

                            }
                            else
                            {
                                c.SetCellValue(text);
                            }
                        }
                        else
                        {
                            c.SetCellValue(text);
                        }
                        colIndex++;

                    }
                }
            }
            byte[] rv = new byte[] { };
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                rv = ms.ToArray();
            }
            return rv;
        }

        /// <summary>
        /// NPOI导出 优化到处20151203
        /// <remarks>通过EntityList导出</remarks>
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateExcelByNPOIV2()
        {
            //数字
            //Regex reg = new Regex(@"^[-]?\d+[.]?\d*$");

            int sheetno = ((EntityList.Count - 1) / 60000) + 1;
            HSSFWorkbook workbook = new HSSFWorkbook();
            List<HSSFSheet> sheets = new List<NPOI.HSSF.UserModel.HSSFSheet>();
            for (int i = 1; i <= sheetno; i++)
            {
                HSSFSheet sheet = workbook.CreateSheet("Sheet" + i) as HSSFSheet;
                HSSFRow dataRow = sheet.CreateRow(0) as HSSFRow;
                MakeExcelHeader(ListColumns, dataRow, 0, 0);
                sheets.Add(sheet);
            }


            int rowIndex = 0;
            int colIndex = 0;
            foreach (var row in EntityList)
            {
                rowIndex++;
                int sheetindex = ((rowIndex - 1) / 60000);
                colIndex = 0;
                //System.Drawing.Color bgColor = System.Drawing.Color.Empty;
                //System.Drawing.Color fColor = System.Drawing.Color.Empty;
                HSSFRow dr = sheets[sheetindex].CreateRow(rowIndex - sheetindex * 60000) as HSSFRow;

                foreach (var baseCol in ListColumns)
                {
                    foreach (var col in baseCol.BottomChildren)
                    {
                        //sheet.Cells.AddValueCell(rowIndex, colIndex, row[col.ColumnName].ToString());//将数据添加到xls表格里
                        //Cell cell= cells.AddValueCell(rowIndex, colIndex, Convert.ToDouble(row[col.ColumnName].ToString()));//转换为数字型
                        string text = col.GetText(row).ToString(); //Regex.Replace(col.GetText(row).ToHtmlString(), @"<[^>]*>", String.Empty);
                        var c = dr.CreateCell(colIndex);
                        double dtext = 0d;

                        if (text != null && double.TryParse(text, out dtext))
                        {
                            if (text.Contains('.') || text == "0" || (text.Length < 6 && !text.StartsWith("0")))
                            {
                                c.SetCellValue(dtext);
                            }
                            else
                            {
                                c.SetCellValue(text); 
                            }
                        }
                        else
                        {
                            c.SetCellValue(text);
                        }

                        colIndex++;

                    }
                }
            }
            byte[] rv = new byte[] { };
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                rv = ms.ToArray();
            }
            return rv;
        }

        private void MakeExcelHeader(List<IGridColumn<TModel>> cols, HSSFRow row, int colIndex, int rowIndex)
        {
            int maxLevel = cols.Max(x => x.MaxLevel);
            foreach (var col in cols)
            {
                row.CreateCell(colIndex).SetCellValue(col.Header);
                //int maxRow = maxLevel - col.MaxLevel + rowIndex;
                int maxCol = colIndex + col.MaxChildrenCount - 1;

                colIndex = maxCol + 1;
            }
        }

        /// <summary>
        /// NPOI导出
        /// <remarks>通过DataTable导出</remarks>
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateExcelByNPOI(DataTable table)
        {
            //数字
            Regex reg = new Regex(@"^[-]?\d+[.]?\d*$");

            int sheetno = ((table.Rows.Count - 1) / 60000) + 1;
            HSSFWorkbook workbook = new HSSFWorkbook();
            
            List<HSSFSheet> sheets = new List<NPOI.HSSF.UserModel.HSSFSheet>();
            for (int i = 1; i <= sheetno; i++)
            {
                HSSFSheet sheet = workbook.CreateSheet("Sheet" + i) as HSSFSheet;
                HSSFRow dataRow = sheet.CreateRow(0) as HSSFRow;
                MakeExcelHeader(table.Columns, dataRow, 0, 0);
                sheets.Add(sheet);
            }


            int rowIndex = 0;
            foreach (DataRow row in table.Rows)
            {
                rowIndex++;
                int sheetindex = ((rowIndex - 1) / 60000);
                System.Drawing.Color bgColor = System.Drawing.Color.Empty;
                System.Drawing.Color fColor = System.Drawing.Color.Empty;
                HSSFRow dr = sheets[sheetindex].CreateRow(rowIndex - sheetindex * 60000) as HSSFRow;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    string text = Regex.Replace(row[i].ToString(), @"<[^>]*>", String.Empty);
                    var c = dr.CreateCell(i);
                    //如果为数字的话
                    if (text != null && reg.IsMatch(text))
                    {
                        if (text.Contains('.') | text == "0" || (text.Length < 6 && !text.StartsWith("0")))
                        {
                            c.SetCellValue(double.Parse(text));

                        }
                        else
                        {
                            c.SetCellValue(text);
                        }
                    }
                    else
                    {
                        c.SetCellValue(text);
                    }
                }
            }
            byte[] rv = new byte[] { };
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                rv = ms.ToArray();
            }
            return rv;
        }

        private void MakeExcelHeader(DataColumnCollection cols, HSSFRow row, int colIndex, int rowIndex)
        {
            foreach (DataColumn col in cols)
            {
                row.CreateCell(colIndex).SetCellValue(col.ColumnName);                
            }
        }
        #endregion

        /// <summary>
        /// 导出pdf
        /// </summary>
        /// <param name="IsHorizontal">是否为横向布局</param>
        /// <param name="TableTitle">查询列表,表头数据</param>
        /// <returns></returns>
        public byte[] GeneratePDF( bool IsHorizontal = false, PdfHelper.PdfTableTitle TableTitle=null)
        {
            //设置pdf宽度
            Rectangle pagesize = null;
            if (IsHorizontal)
            {
                pagesize=new Rectangle(PageSize.A4.Height,PageSize.A4.Width );
                
            }
            else
            {
                pagesize= new Rectangle(PageSize.A4.Width, PageSize.A4.Height);
            }
            Document pdf = new Document(pagesize);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer= PdfWriter.GetInstance(pdf, ms);
            BaseFont bfSun = BaseFont.CreateFont(@"c:\windows\fonts\SIMSUN.TTC,0", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
          
            iTextSharp.text.Font headerFont = new iTextSharp.text.Font(bfSun, 9);
            iTextSharp.text.Font normalFont = new iTextSharp.text.Font(bfSun, 8);
            pdf.Open();
            
            //如果查询字符串不为空，则加表头
            if (TableTitle != null)
            {
                //设置内容的左右上下margin
                pdf.SetMargins(10, 10, 105, 30);

                var pEvent = new PdfHelper.PDFEvent(bfSun, pagesize) { TableTitle = TableTitle};
                writer.PageEvent = pEvent;
                //pEvent.CreateHeaderTemplate(write);
                
            }
            else
            {
                pdf.SetMargins(10, 10, 20, 20);
            }
            pdf.NewPage();
            #region 导出数据表格
            PdfPTable table = new PdfPTable(ListColumns.Sum(x => x.MaxChildrenCount));
            table.KeepTogether = true;
            table.TotalWidth = pagesize.Width;
            table.WidthPercentage = 100;
            table.DefaultCell.Border = 0;
            table.DefaultCell.Padding = 1;
            MakePdfHeader(ListColumns, table, headerFont);
            Dictionary<IGridColumn<TModel>, int> sameCount = new Dictionary<IGridColumn<TModel>, int>();
            foreach (var baseCol in ListColumns)
            {
                foreach (var col in baseCol.BottomChildren)
                {
                    sameCount.Add(col, 0);
                }
            }
            //int i = 0;
            //foreach (var row in EntityList)
            for(int i=0;i<EntityList.Count;i++)
            {
                var row = EntityList[i];
                foreach (var baseCol in ListColumns)
                {
                    foreach (var col in baseCol.BottomChildren)
                    {
                        System.Drawing.Color backColor = col.GetBackGroundColor(row);
                        System.Drawing.Color foreColor = col.GetForeGroundColor(row);
                        if (foreColor != System.Drawing.Color.Empty)
                        {
                            normalFont.Color = new BaseColor(foreColor);
                        }
                        PdfPCell cell = new PdfPCell(new Phrase(col.GetText(row).ToHtmlString(), normalFont)) { Border=0,Padding=1};
                        if (backColor != System.Drawing.Color.Empty)
                        {
                            cell.BackgroundColor = new BaseColor(backColor);
                        }
                        if (col.NeedGroup == true && sameCount[col] == i)
                        {
                            sameCount[col] = sameCount[col] + 1;
                            string lastValue = col.GetText(row).ToHtmlString();
                            for (int j = i + 1; j < EntityList.Count; j++)
                            {
                                if (col.GetText(EntityList[j]).ToHtmlString() == lastValue)
                                {
                                    sameCount[col] = sameCount[col] + 1;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            cell.Rowspan = sameCount[col] - i;
                            table.AddCell(cell);
                        }
                        if (col.NeedGroup == false)
                        {
                            table.AddCell(cell);
                        }
                    }
                }
                //如果有表头，需要每一页又要有表头
                if (TableTitle != null && TableTitle.Count > 0 && (i+1) % TableTitle.Count == 0)
                {
                    pdf.Add(table);
                    pdf.NewPage();
                    table.Rows.Clear();
                    MakePdfHeader(ListColumns, table, headerFont);
                }
                //i++;
            }
            if (table.Rows.Count > 0)
            {
                pdf.Add(table);
            }
            #endregion
            
            pdf.Close();
            byte[] rv = ms.ToArray();
            ms.Close();
            return rv;
        }

        private void MakePdfHeader(List<IGridColumn<TModel>> columns, PdfPTable table, iTextSharp.text.Font headerFont)
        {
            int maxLevel = columns.Max(x => x.MaxLevel);
            foreach (var col in columns)
            {
                PdfPCell cell = null;
                cell = new PdfPCell(new Phrase(col.Header, headerFont)) { Border=0,Padding=1};
               
                cell.Colspan = col.MaxChildrenCount;
                cell.Rowspan = (maxLevel - col.MaxLevel + 1);
                table.AddCell(cell);
            }
            foreach (var col in columns)
            {
                if (col.Children != null && col.Children.Count > 0)
                {
                    MakePdfHeader(col.Children, table, headerFont);
                }
            }
        }


        /// <summary>
        /// 获取pdf行
        /// </summary>
        /// <param name="font"></param>
        /// <param name="texts"></param>
        /// <returns></returns>
        public static PdfPRow GetRows(iTextSharp.text.Font font, params object[] texts)
        {
            List<PdfPCell> pcells = new List<PdfPCell>();
            foreach (var text in texts)
            {
                string t = text == null ? "" : text.ToString();
                PdfPCell pcell = new PdfPCell(new Phrase(t, font));
                pcell.Border = 0;
                pcells.Add(pcell);
            }

            PdfPRow prow = new PdfPRow(pcells.ToArray());
            return prow;
        }

        /// <summary>
        /// 将字符串显示为较短的字符文本
        /// </summary>
        /// <param name="name">要显示的文本</param>
        /// <param name="length">长度</param>
        /// <returns>返回span标记的tagbuilder</returns>
        public MvcHtmlString FormatName(string name,  int length =15)
        {
            name = System.Web.HttpUtility.HtmlDecode(name);
            if (!String.IsNullOrEmpty(name))
            {
                TagBuilder tg = new TagBuilder("span");
                tg.MergeAttribute("title", name);
                if (name.Length > length + 1)
                {
                    tg.InnerHtml = name.Substring(0, length) + "…";
                }
                else
                {
                    tg.InnerHtml = name;
                }
                return MvcHtmlString.Create(tg.ToString());
            }
            return MvcHtmlString.Create("");
        }

    }

    public class GridColumn<T> : IGridColumn<T> where T : class
    {
        public Expression<Func<T, object>> ColumnNameExp { get; set; }
        public List<IGridColumn<T>> Children { get; set; }
        private string _header;
        public string Header
        {
            get
            {
                if (string.IsNullOrEmpty(_header))
                {
                    return GetHeader();
                }
                else
                {
                    return _header;
                }
            }
            set { _header = value; }
        }
        public virtual string ColumnName
        {
            get { return UtilsTool.GetPropertyName(ColumnNameExp); }
        }
        public Func<T, dynamic, object> Format { get; set; }
        public int? Width { get; set; }
        public bool NeedGroup { get; set; }
        public bool CanSort { get; set; }
        public Func<T, System.Drawing.Color> ForeGroundFunc { get; set; }
        public Func<T, System.Drawing.Color> BackGroundFunc { get; set; }

        public int MaxChildrenCount
        {
            get
            {
                int rv = 1;
                if (this.Children != null && this.Children.Count > 0)
                {
                    rv = 0;
                    foreach (var child in this.Children)
                    {
                        rv += child.MaxChildrenCount;
                    }
                }
                return rv;
            }
        }

        public int MaxLevel
        {
            get
            {
                int rv = 1;
                if (this.Children != null && this.Children.Count > 0)
                {
                    int max = 0;
                    foreach (var child in this.Children)
                    {
                        int temp = child.MaxLevel;
                        if (max < temp)
                        {
                            max = temp;
                        }
                    }
                    rv += max;
                }
                return rv;

            }
        }

        public List<IGridColumn<T>> BottomChildren
        {
            get
            {
                List<IGridColumn<T>> rv = new List<IGridColumn<T>>();
                if (Children != null && Children.Count > 0)
                {
                    foreach (var child in Children)
                    {
                        rv.AddRange(child.BottomChildren);
                    }
                }
                else
                {
                    rv.Add(this);
                }
                return rv;
            }
        }

        public System.Drawing.Color GetForeGroundColor(T source)
        {
            if (ForeGroundFunc == null)
            {
                return System.Drawing.Color.Empty;
            }
            else
            {
                return ForeGroundFunc.Invoke(source);
            }
        }

        public System.Drawing.Color GetBackGroundColor(T source)
        {
            if (BackGroundFunc == null)
            {
                return System.Drawing.Color.Empty;
            }
            else
            {
                return BackGroundFunc.Invoke(source);
            }
        }

        public virtual MvcHtmlString GetText(T source)
        {
            string rv = "";
            object temp = null;
            try
            {
                temp = ColumnNameExp.Compile()(source);
            }
            catch { }
            rv = temp == null ? "" : temp.ToString();
            if (Format != null)
            {
                rv = Format.Invoke(source, temp).ToString();
            }
            else
            {
            }
            return MvcHtmlString.Create(rv);
        }

        public virtual Type GetColumnType(Type ClassType)
        {
            Type rv = ClassType.GetProperty(UtilsTool.GetPropertyName(ColumnNameExp)).PropertyType;
            return rv;
        }

        protected virtual string GetHeader()
        {
            string rv = "";
            var dis = UtilsTool.GetPropertyInfo(ColumnNameExp).GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            if (dis != null)
            {
                if (dis.ResourceType == null)
                {
                    rv = dis.Name;
                }
                else
                {
                    rv = Resources.Language.ResourceManager.GetString(dis.Name);
                }
            }
            return rv;
        }
    }

    public class MLGridColumn<T, V, ML> : GridColumn<T>
        where T : class
        where V : class,IMLData<ML>
        where ML : MLContent
    {
        public override string ColumnName
        {
            get { return UtilsTool.GetPropertyName(LanguageFormat); }
        }

        public MLGridColumn(Expression<Func<T, V>> MiddleField, Expression<Func<ML, string>> LanguageFormat)
        {
            this.MiddleField = MiddleField;
            this.LanguageFormat = LanguageFormat;
        }

        public Expression<Func<T, V>> MiddleField { get; set; }

        public Expression<Func<ML, string>> LanguageFormat { get; set; }

        public override MvcHtmlString GetText(T source)
        {
            string rv = "";
            SupportedLanguage CurrentLanguage = UtilsTool.GetCurrentLanguage();
            var middle = MiddleField.Compile().Invoke(source);
            //renyj 2011-6-28
            if (middle == null)
            {
                return MvcHtmlString.Create(rv);
            }
            if (middle.MLContents != null)
            {
                rv = middle.GetLanguageSpecificContent(LanguageFormat);//.MLContents .Where(x => x.LanguageCode == CurrentLanguage.LanguageCode).Select(LanguageFormat.Compile()).FirstOrDefault();
            }
            if (Format != null)
            {
                rv = Format.Invoke(source, rv).ToString();
            }
            return MvcHtmlString.Create(rv);
        }

        public override Type GetColumnType(Type ClassType)
        {
            return typeof(string);
        }

        protected override string GetHeader()
        {
            string rv = "";
            var dis = typeof(ML).GetProperty(UtilsTool.GetPropertyName(LanguageFormat)).GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            if (dis != null)
            {
                if (dis.ResourceType == null)
                {
                    rv = dis.Name;
                }
                else
                {
                    rv = Resources.Language.ResourceManager.GetString(dis.Name);
                }
            }
            return rv;
        }

        
    }

}