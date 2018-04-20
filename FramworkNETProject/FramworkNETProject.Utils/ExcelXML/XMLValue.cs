using System;

namespace Utils.ExcelXML
{
    public static class XMLValue
    {
        public static readonly string Header = @"<?xml version='1.0'?><?mso-application progid='Excel.Sheet'?><Workbook xmlns='urn:schemas-microsoft-com:office:spreadsheet' xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns:ss='urn:schemas-microsoft-com:office:spreadsheet' xmlns:html='http://www.w3.org/TR/REC-html40'><DocumentProperties xmlns='urn:schemas-microsoft-com:office:office'><Version>12.00</Version></DocumentProperties><Styles>
<Style ss:ID='Default' ss:Name='Normal'><Alignment ss:Vertical='Bottom'/><Borders/><Font ss:FontName='Arial' x:Family='Swiss'/><Interior/><NumberFormat/><Protection/></Style>
<Style ss:ID='sh'><Alignment ss:Horizontal='Center' ss:Vertical='Center'/><Borders><Border ss:Position='Bottom' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Left' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Right' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Top' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/></Borders><Font ss:FontName='Arial' x:Family='Swiss' ss:Bold='1'/><Interior/><NumberFormat/><Protection/></Style>
<Style ss:ID='sc'><Borders><Border ss:Position='Bottom' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Left' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Right' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Top' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/></Borders><Font ss:FontName='Arial' x:Family='Swiss'/><Interior/><NumberFormat/><Protection/></Style>
<Style ss:ID='sd'><Alignment ss:Vertical='Center'/><Font ss:FontName='Arial' x:Family='Swiss'/><NumberFormat ss:Format='yyyy\-m\-d\ h:mm:ss'/><Borders><Border ss:Position='Bottom' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Left' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Right' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Top' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/></Borders></Style>
</Styles><Worksheet ss:Name='SheetResult'><Table>";
        public static readonly string Bottom = @"</Table></Worksheet></Workbook>";



    }

    public static class XMLMoreSheetValue
    {
        public static string sheetName = "SheetResult";
        public static string Header = @"<?xml version='1.0'?><?mso-application progid='Excel.Sheet'?><Workbook xmlns='urn:schemas-microsoft-com:office:spreadsheet' xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns:ss='urn:schemas-microsoft-com:office:spreadsheet' xmlns:html='http://www.w3.org/TR/REC-html40'><DocumentProperties xmlns='urn:schemas-microsoft-com:office:office'><Version>12.00</Version></DocumentProperties><Styles>
<Style ss:ID='Default' ss:Name='Normal'><Alignment ss:Vertical='Bottom'/><Borders/><Font ss:FontName='Arial' x:Family='Swiss'/><Interior/><NumberFormat/><Protection/></Style>
<Style ss:ID='sh'><Alignment ss:Horizontal='Center' ss:Vertical='Center'/><Borders><Border ss:Position='Bottom' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Left' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Right' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Top' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/></Borders><Font ss:FontName='Arial' x:Family='Swiss' ss:Bold='1'/><Interior/><NumberFormat/><Protection/></Style>
<Style ss:ID='sc'><Borders><Border ss:Position='Bottom' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Left' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Right' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Top' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/></Borders><Font ss:FontName='Arial' x:Family='Swiss'/><Interior/><NumberFormat/><Protection/></Style>
<Style ss:ID='sd'><Alignment ss:Vertical='Center'/><Font ss:FontName='Arial' x:Family='Swiss'/><NumberFormat ss:Format='yyyy\-m\-d\ h:mm:ss'/><Borders><Border ss:Position='Bottom' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Left' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Right' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/><Border ss:Position='Top' ss:LineStyle='Continuous' ss:Weight='1' ss:Color='#000000'/></Borders></Style>
</Styles>";
        public static readonly string Bottom = @"</Workbook>";



    }
}
