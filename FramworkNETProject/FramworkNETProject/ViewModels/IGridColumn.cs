using System;
using System.Collections.Generic;
namespace ViewModels
{
    public interface IGridColumn<T>
    {
        string ColumnName { get; }
        Func<T, dynamic, object> Format { get; set; }
        List<IGridColumn<T>> Children { get; }
        List<IGridColumn<T>> BottomChildren { get; }
        System.Web.Mvc.MvcHtmlString GetText(T source);
        System.Drawing.Color GetForeGroundColor(T source);
        System.Drawing.Color GetBackGroundColor(T source);
        Type GetColumnType(Type ClassType);
        string Header { get; set; }
        bool NeedGroup { get; set; }
        int? Width { get; set; }
        int MaxChildrenCount { get; }
        int MaxLevel { get; }
    }
}
