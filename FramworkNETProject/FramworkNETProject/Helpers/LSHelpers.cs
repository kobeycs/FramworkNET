using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Models;
using ViewModels;
using Resources;
using SupportClasses;
using Controllers;
using Models.System;

namespace Helpers
{
    public static class LSHelpers
    {
        /// <summary>
        /// �������ֶα༭�ؼ�
        /// </summary>
        /// <typeparam name="TModel">ViewModel</typeparam>
        /// <typeparam name="TContent">����������</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="entityExp">��ȡ����������������ı��ʽ</param>
        /// <param name="mlExp">��ȡ���������ݵı��ʽ</param>
        /// <param name="memberExp">��ȡ������������Ҫ�༭�ֶεı��ʽ</param>
        /// <returns>�ؼ���HTML</returns>
        public static MvcHtmlString LS_MLInput<TViewModel, TEntity, TContent>(
            this HtmlHelper<TViewModel> html,
            Expression<Func<TViewModel, TEntity>> entityExp,
            Expression<Func<TEntity, List<TContent>>> mlExp,
            Expression<Func<TContent, object>> memberExp,
            string DialogID = null)
            where TEntity : BasePoco
            where TViewModel : BaseVM
            where TContent : MLContent
        {
            //��ȡ��ǰ����
            SupportedLanguage CurrentLanguage = UtilsTool.GetCurrentLanguage();
            //����ϵͳ��֧�ֵ����ԣ�����ǰ����������ǰ
            List<SupportedLanguage> lans = new List<SupportedLanguage>();
            lans.Add(CurrentLanguage);
            lans.AddRange(BaseController.Languages.Where(x => x.LanguageCode.ToLower() != CurrentLanguage.LanguageCode.ToLower()));

            //��ȡ�����Ե�����
            var entity = entityExp.Compile()(html.ViewData.Model);
            var textValues = mlExp.Compile()(entity);

            var entityName = UtilsTool.GetPropertyName(entityExp);
            //��ȡҪ�༭�ֶε�����
            var fieldName = UtilsTool.GetPropertyName(memberExp);
            //��ȡ�������ֶε�·������
            var longName = entityName + "." + UtilsTool.GetPropertyName(mlExp);

            //���ֶζ�Ӧ�����Ժ����ݴ����ֵ���
            Dictionary<string, string> values = new Dictionary<string, string>();
            Dictionary<string, long> ids = new Dictionary<string, long>();
            if (textValues != null)
            {
                foreach (var item in textValues)
                {
                    var v = item.GetType().GetProperty(fieldName).GetValue(item, null);
                    var id = (long)item.GetType().GetProperty("ID").GetValue(item, null);
                    values.Add(item.LanguageCode, v == null ? "" : v.ToString());
                    ids.Add(item.LanguageCode, id);
                }
            }
            //���ϵͳ��֧�ֵ�������û�и��ֶε����ݣ���Ϊ���ַ���
            foreach (var item in lans)
            {
                if (!values.ContainsKey(item.LanguageCode))
                {
                    values.Add(item.LanguageCode, "");
                    ids.Add(item.LanguageCode, 0);
                }
            }

            //���������Html
            TagBuilder mainDiv = new TagBuilder("div");

            TagBuilder dialogScript = new TagBuilder("script");
            dialogScript.MergeAttribute("type", "text/javascript");

            TagBuilder tabDiv = new TagBuilder("div");
            tabDiv.MergeAttribute("id", fieldName + "tabs");
            tabDiv.MergeAttribute("style", "display:none");

            TagBuilder contentDiv = new TagBuilder("div");
            //���һ��ֻ�����ؼ�����ʾ��ǰ���Ե�����
            TagBuilder disabledTextbox = new TagBuilder("input");
            disabledTextbox.MergeAttribute("type", "text");
            disabledTextbox.MergeAttribute("id", longName.Replace(".", "_") + "" + fieldName);
            disabledTextbox.MergeAttribute("name", longName + "" + fieldName);
            disabledTextbox.MergeAttribute("data-val", "true");
            disabledTextbox.MergeAttribute("readonly", "readonly");
            disabledTextbox.MergeAttribute("value", values[CurrentLanguage.LanguageCode]);
            disabledTextbox.MergeAttribute("data-fname", fieldName);

            //�����֤��Ϣ
            TagBuilder span = new TagBuilder("span");
            span.MergeAttribute("data-valmsg-for", longName + "" + fieldName);
            span.MergeAttribute("data-valmsg-replace", "true");

            if (html.ViewData.ModelState[longName + "" + fieldName] != null && html.ViewData.ModelState[longName + "" + fieldName].Errors.Count > 0)
            {
                disabledTextbox.MergeAttribute("class", "input-validation-error");
                span.InnerHtml = html.ViewData.ModelState[longName + "" + fieldName].Errors[0].ErrorMessage;
                span.MergeAttribute("class", "field-validation-error");
            }


            TagBuilder ul = new TagBuilder("ul");
            ul.MergeAttribute("style", "border-radius: 5px 5px 0px 0px;");
            TagBuilder holderDiv = new TagBuilder("div");
            TagBuilder hiddenDiv = new TagBuilder("div");

            //ѭ���������ԣ�Ϊÿ���������һ��Tab����һ��TextBox�����༭
            for (int i = 0; i < lans.Count; i++)
            {
                TagBuilder subDiv = new TagBuilder("div");
                subDiv.MergeAttribute("id", "mltabs-" + i);
                subDiv.MergeAttribute("style", "border-width:0 1px 1px 1px;");

                //����һ������������ı���Textbox
                TagBuilder textbox = new TagBuilder("input");
                textbox.MergeAttribute("type", "text");
                textbox.MergeAttribute("id", "dag" + longName.Replace(".", "_") + "_" + i + "__" + fieldName);
                textbox.MergeAttribute("name", "dag" + longName + "[" + i + "]." + fieldName);
                textbox.MergeAttribute("value", values[lans[i].LanguageCode]);
                textbox.MergeAttribute("data-languagecode", lans[i].LanguageCode);

                //����һ������������ı������ر���
                TagBuilder hiddentext = new TagBuilder("input");
                hiddentext.MergeAttribute("type", "hidden");
                hiddentext.MergeAttribute("id", longName.Replace(".", "_") + "_" + i + "__" + fieldName);
                hiddentext.MergeAttribute("name", longName + "[" + i + "]." + fieldName);
                hiddentext.MergeAttribute("value", values[lans[i].LanguageCode]);
                hiddentext.MergeAttribute("data-languagedef", lans[i].LanguageCode);

                //����һ����������code�����ر���
                TagBuilder hiddenLan = new TagBuilder("input");
                hiddenLan.MergeAttribute("type", "hidden");
                hiddenLan.MergeAttribute("id", longName.Replace(".", "_") + "_" + i + "__LanguageCode");
                hiddenLan.MergeAttribute("name", longName + "[" + i + "].LanguageCode");
                hiddenLan.MergeAttribute("value", lans[i].LanguageCode);

                //����һ����������ID�����ر���
                TagBuilder hiddenid = new TagBuilder("input");
                hiddenid.MergeAttribute("type", "hidden");
                hiddenid.MergeAttribute("id", longName.Replace(".", "_") + "_" + i + "__ID");
                hiddenid.MergeAttribute("name", longName + "[" + i + "].ID");
                hiddenid.MergeAttribute("value", ids[lans[i].LanguageCode].ToString());

                //����һ�������������ظ����͵�ID
                TagBuilder hiddenentityid = new TagBuilder("input");
                hiddenentityid.MergeAttribute("type", "hidden");
                hiddenentityid.MergeAttribute("id", longName.Replace(".", "_") + "_" + i + "__MLParentID");
                hiddenentityid.MergeAttribute("name", longName + "[" + i + "].MLParentID");
                hiddenentityid.MergeAttribute("value", entity.ID.ToString());

                subDiv.InnerHtml = textbox.ToString(TagRenderMode.SelfClosing);
                holderDiv.InnerHtml += subDiv.ToString();

                TagBuilder li = new TagBuilder("li");
                TagBuilder a = new TagBuilder("a");
                a.MergeAttribute("href", "#mltabs-" + i);
                a.SetInnerText(lans[i].LanguageName);
                li.InnerHtml = a.ToString();
                li.MergeAttribute("style", "font-size:11px");
                ul.InnerHtml += li.ToString();

                hiddenDiv.InnerHtml += hiddentext.ToString(TagRenderMode.SelfClosing);
                hiddenDiv.InnerHtml += hiddenLan.ToString(TagRenderMode.SelfClosing);
                hiddenDiv.InnerHtml += hiddenid.ToString(TagRenderMode.SelfClosing);
                hiddenDiv.InnerHtml += hiddenentityid.ToString(TagRenderMode.SelfClosing);
            }

            tabDiv.InnerHtml += ul.ToString();
            tabDiv.InnerHtml += holderDiv.InnerHtml;

            string script = "";
            if (string.IsNullOrEmpty(DialogID))
            {
                script = string.Format("FF_InitMLInput (\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\");", longName.Replace(".", "_"), fieldName, Resources.Language.ȷ��, Resources.Language.ȡ��, tabDiv.ToString().Replace("\"", "'"));
            }
            else
            {
                script = string.Format("FF_InitMLInput (\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\");", longName.Replace(".", "_"), fieldName, Resources.Language.ȷ��, Resources.Language.ȡ��, tabDiv.ToString().Replace("\"", "'"), DialogID);
            }
            dialogScript.InnerHtml = script;

            contentDiv.InnerHtml = disabledTextbox.ToString(TagRenderMode.SelfClosing);
            contentDiv.InnerHtml += dialogScript.ToString();

            mainDiv.InnerHtml += contentDiv.ToString();
            mainDiv.InnerHtml += hiddenDiv.ToString();

            return new MvcHtmlString(mainDiv.ToString() + span.ToString());
        }

        /// <summary>
        /// �����б�ؼ�
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel</typeparam>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="entityList">�����б�</param>
        /// <param name="columns">Ҫ��ʾ����</param>
        /// <param name="searcher">Ҫʹ�õ�������</param>
        /// <param name="tableStyle">���ɱ�����ʽ</param>
        /// <param name="headerStyle">���ͷ��ʽ</param>
        /// <param name="footerStyle">���β��ʽ</param>
        /// <param name="alternatingRowStyle">������ʽ</param>
        /// <param name="selectedRowStyle">ѡ������ʽ</param>
        /// <param name="rowStyle">��ͨ����ʽ</param>
        /// <returns>�ؼ���HTML</returns>
        public static MvcHtmlString LS_Grid<TViewModel, TModel>(
            this HtmlHelper<TViewModel> html,
            List<TModel> entityList,
            List<IGridColumn<TModel>> columns,
            BaseSearcher searcher,
            string tableStyle = "GridTable",
            string headerStyle = "GridTableHeaderRow",
            string footerStyle = "digg",
            string alternatingRowStyle = "webgrid-alternating-row",
            string selectedRowStyle = "webgrid-selected-row",
            string rowStyle = "webgrid-row-style")
            where TViewModel : BaseVM
            where TModel : class
        {
            //TagBuilder outerDiv = new TagBuilder("div");
            //TagBuilder innerDiv = new TagBuilder("div");
            //outerDiv.MergeAttribute("style", "width:100%;overflow-x:scroll;height:100%;");
            //innerDiv.MergeAttribute("style", "width:200%;");

            TagBuilder mainDiv = new TagBuilder("div");
            TagBuilder tableDiv = new TagBuilder("div");
            TagBuilder pagerDiv = new TagBuilder("div");
            string divid = Guid.NewGuid().ToString();
            pagerDiv.MergeAttribute("id", divid);
            pagerDiv.MergeAttribute("class", footerStyle);
            pagerDiv.MergeAttribute("align", "center");

            TagBuilder table = new TagBuilder("table");
            table.MergeAttribute("class", tableStyle);
            table.MergeAttribute("width", "100%");
            table.MergeAttribute("cellspacing", "0");
            table.MergeAttribute("cellpadding", "0");
            table.MergeAttribute("border", "0");

            //table.MergeAttribute("height", "400px");

            TagBuilder thead = new TagBuilder("thead");

            if (columns != null)
            {
                thead.InnerHtml += GetGridHeader<TModel>(columns, headerStyle);
            }
            TagBuilder tbody = new TagBuilder("tbody");
            tbody.MergeAttribute("style", "min-height:400px; ");

            int i = 0;
            //ѭ���������
            Dictionary<IGridColumn<TModel>, int> sameCount = new Dictionary<IGridColumn<TModel>, int>();
            foreach (var baseCol in columns)
            {
                foreach (var col in baseCol.BottomChildren)
                {
                    sameCount.Add(col, 0);
                }
            }
            foreach (var sou in entityList)
            {
                TagBuilder innertr = new TagBuilder("tr");
                innertr.MergeAttribute("class", (i % 2 == 0 ? rowStyle : alternatingRowStyle));
                foreach (var baseCol in columns)
                {
                    foreach (var col in baseCol.BottomChildren)
                    {
                        TagBuilder innertd = new TagBuilder("td");
                        System.Drawing.Color backColor = col.GetBackGroundColor(sou);
                        System.Drawing.Color foreColor = col.GetForeGroundColor(sou);
                        if (backColor != System.Drawing.Color.Empty)
                        {
                            innertd.MergeAttribute("style", "background-color:" + System.Drawing.ColorTranslator.ToHtml(backColor));
                        }
                        if (foreColor != System.Drawing.Color.Empty)
                        {
                            innertd.MergeAttribute("style", "color:" + System.Drawing.ColorTranslator.ToHtml(foreColor));
                        }
                        if (col.NeedGroup == true && sameCount[col] == i)
                        {
                            sameCount[col] = sameCount[col] + 1;
                            string lastValue = col.GetText(sou).ToHtmlString();
                            for (int j = i + 1; j < entityList.Count; j++)
                            {
                                if (col.GetText(entityList[j]).ToHtmlString() == lastValue)
                                {
                                    sameCount[col] = sameCount[col] + 1;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            innertd.MergeAttribute("rowspan", (sameCount[col] - i).ToString());
                            innertd.InnerHtml = lastValue;
                            innertr.InnerHtml += innertd.ToString();
                        }
                        if (col.NeedGroup == false)
                        {
                            innertd.InnerHtml = col.GetText(sou).ToHtmlString();
                            innertr.InnerHtml += innertd.ToString();
                        }
                        if (col.Width != null)
                        {
                            innertd.MergeAttribute("style", "width:" + col.Width.Value + "px");
                        }
                    }
                }
                i++;
                tbody.InnerHtml += innertr.ToString();
            }


            if (entityList.Count == 0)
            {
                TagBuilder innertr = new TagBuilder("tr");
                TagBuilder innertd = new TagBuilder("td");
                innertd.InnerHtml = "&nbsp";

                if (columns != null)
                {
                    innertd.MergeAttribute("colspan", columns.Count.ToString());
                }
                innertr.InnerHtml += innertd.ToString();
                tbody.InnerHtml += innertr.ToString();
            }


            if (columns != null)
            {
                table.InnerHtml += thead;
            }
            table.InnerHtml += tbody;
            tableDiv.InnerHtml += table.ToString();

            //��ҳ����
            TagBuilder hidden = new TagBuilder("input");
            hidden.MergeAttribute("type", "hidden");
            hidden.MergeAttribute("id", "Searcher_CurrentPage");
            hidden.MergeAttribute("name", "Searcher.CurrentPage");
            hidden.MergeAttribute("value", searcher.CurrentPage.ToString());
            pagerDiv.InnerHtml += hidden.ToString(TagRenderMode.SelfClosing);
            if (searcher.TotalPages > 1)
            {


                #region �����ҳ��ʽ

                int currentPage = Convert.ToInt32(searcher.CurrentPage);
                int currentPageSize = 10;
                int totalPage = searcher.TotalPages;

                int seed = currentPage % currentPageSize == 0 ? currentPage : currentPage - (currentPage % currentPageSize);

                TagBuilder pageTable = new TagBuilder("table");
                TagBuilder pagetbody = new TagBuilder("tbody");
                TagBuilder pageTr = new TagBuilder("tr");
                pageTr.MergeAttribute("valign", "top");


                //�����ҳ
                TagBuilder firstTd = new TagBuilder("td");
                TagBuilder firstpage = new TagBuilder("a");

                firstpage.MergeAttribute("href", "#");
                firstpage.MergeAttribute("onclick", "javascript:GoToPage(1,this)");
                firstpage.InnerHtml = Language.��ҳ;

                firstTd.InnerHtml = firstpage.ToString();
                pageTr.InnerHtml = firstTd.ToString();

                //�����ǰҳ�����ǵ�һҳ������ʾ��һҳ��ť
                if (currentPage >= 1)
                {
                    TagBuilder previewTd = new TagBuilder("td");
                    TagBuilder a = new TagBuilder("a");
                    a.MergeAttribute("href", "#");
                    a.MergeAttribute("onclick", "javascript:GoToPage(" + currentPage.ToString() + ",this)");
                    a.InnerHtml = Language.��һҳ;
                    previewTd.InnerHtml += a.ToString();
                    pageTr.InnerHtml += previewTd.ToString();


                }
                if (currentPage - currentPageSize >= 0)
                {
                    TagBuilder restTd = new TagBuilder("td");
                    TagBuilder a = new TagBuilder("a");
                    a.MergeAttribute("href", "#");
                    a.MergeAttribute("onclick", "javascript:GoToPage(" + ((currentPage - currentPageSize) + 1).ToString() + ",this)");
                    a.InnerHtml = "...";
                    //pagerDiv.InnerHtml += a.ToString();
                    restTd.InnerHtml += a.ToString();
                    pageTr.InnerHtml += restTd.ToString();


                }

                for (int x = seed; x < totalPage && x < (seed + currentPageSize); x++)
                {

                    TagBuilder pagenumTd = new TagBuilder("td");
                    if (x == searcher.CurrentPage) //��ǰҳ����ʾ��������ʽ
                    {

                        pagenumTd.InnerHtml += "<span class='current'>" + (x + 1).ToString() + "</span>";
                        pageTr.InnerHtml += pagenumTd.ToString();
                        //pagerDiv.InnerHtml += "<span class='current'>" + (x + 1).ToString() + "</span>";
                    }
                    else
                    {
                        TagBuilder a = new TagBuilder("a");
                        a.MergeAttribute("href", "#");
                        a.MergeAttribute("onclick", "javascript:GoToPage(" + (x + 1).ToString() + ",this)");
                        a.InnerHtml = (x + 1).ToString();
                        //pagerDiv.InnerHtml += a.ToString();
                        pagenumTd.InnerHtml += a.ToString();
                        pageTr.InnerHtml += pagenumTd.ToString();
                    }

                }

                if ((currentPage + currentPageSize) <= (totalPage - 1))
                {

                    TagBuilder otherTd = new TagBuilder("td");
                    TagBuilder a = new TagBuilder("a");
                    a.MergeAttribute("href", "#");
                    a.MergeAttribute("onclick", "javascript:GoToPage(" + (currentPage + currentPageSize + 1).ToString() + ",this)");
                    a.InnerHtml = "...";
                    otherTd.InnerHtml += a.ToString();
                    pageTr.InnerHtml += otherTd.ToString();

                    //pagerDiv.InnerHtml += a.ToString();
                }
                if (currentPage < (totalPage - 1))
                {

                    TagBuilder nextTd = new TagBuilder("td");

                    int nextpage = currentPage + 2;
                    TagBuilder a = new TagBuilder("a");
                    a.MergeAttribute("href", "#");
                    a.MergeAttribute("onclick", "javascript:GoToPage(" + nextpage.ToString() + ",this)");
                    a.InnerHtml = Language.��һҳ;

                    nextTd.InnerHtml += a.ToString();
                    pageTr.InnerHtml += nextTd.ToString();

                    //pagerDiv.InnerHtml += a.ToString();

                }
                //���ĩҳ

                TagBuilder lastTd = new TagBuilder("td");

                TagBuilder lastpage = new TagBuilder("a");
                lastpage.MergeAttribute("href", "#");
                lastpage.MergeAttribute("onclick", "javascript:GoToPage(" + totalPage + ",this)");
                lastpage.InnerHtml = Language.ĩҳ;

                lastTd.InnerHtml += lastpage.ToString();
                pageTr.InnerHtml += lastTd.ToString();

                //pagerDiv.InnerHtml += lastpage.ToString();

                //����ܼ�ҳ��

                TagBuilder totalTd = new TagBuilder("td");
                totalTd.InnerHtml += "<span  >" + Language.�� + totalPage + Language.ҳ + "</span>";
                pageTr.InnerHtml += totalTd.ToString();

                //pagerDiv.InnerHtml += "<span  >" + totalPage + Language.ҳ+ "</span>";


                //����ı�����ת��ť

                TagBuilder jumpTd = new TagBuilder("td");

                TagBuilder txtbox = new TagBuilder("input");
                txtbox.MergeAttribute("type", "text");
                //txtbox.MergeAttribute("size", "3");
                txtbox.MergeAttribute("id", "pagenum");
                txtbox.MergeAttribute("style", "width:60px");

                //pagerDiv.InnerHtml += txtbox.ToString();
                jumpTd.InnerHtml += txtbox.ToString();
                pageTr.InnerHtml += jumpTd.ToString();

                TagBuilder goTd = new TagBuilder("td");
                TagBuilder ato = new TagBuilder("a");
                ato.MergeAttribute("href", "#");
                ato.MergeAttribute("onclick", "javascript:GoToPage($('#pagenum').val(),this)");
                ato.InnerHtml = "GO";

                goTd.InnerHtml += ato.ToString();
                pageTr.InnerHtml += goTd.ToString();
                pagetbody.InnerHtml += pageTr.ToString();
                //pagerDiv.InnerHtml += ato.ToString();
                pageTable.InnerHtml += pagetbody.ToString();
                pagerDiv.InnerHtml += pageTable.ToString();
                #endregion
                //for (int j = 1; j <= searcher.TotalPages; j++)
                //{
                //    if (j == searcher.CurrentPage + 1)
                //    {
                //        pagerDiv.InnerHtml += j + "  ";
                //    }
                //    else
                //    {
                //        TagBuilder a = new TagBuilder("a");
                //        a.MergeAttribute("href", "#");
                //        a.MergeAttribute("onclick", "javascript:GoToPage(" + j + ",this)");
                //        a.InnerHtml = j.ToString();
                //        pagerDiv.InnerHtml += a.ToString();
                //    }
                //}
            }
            else if (entityList.Count == 0)
            {
                TagBuilder p = new TagBuilder("p");
                p.InnerHtml = Resources.Language.�޽��;
                pagerDiv.InnerHtml += p.ToString();
            }
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"function GoToPage(p,e) {
               if (isNaN(p)) {
                  alert('" + Language.���������� + @"!');
                  return false;
                }
                if (p>0)
                {
                    $('#" + divid + @"').find('input:hidden').val(p - 1);    
                    $(e).parents('form:first').submit();
                }
            }";
    //$('form:has(#Searcher_CurrentPage)').submit();
            pagerDiv.InnerHtml += script.ToString();
            mainDiv.InnerHtml += tableDiv.ToString();
            if (searcher.CurrentPage >= 0)
            {
                mainDiv.InnerHtml += pagerDiv.ToString();
            }


            //innerDiv.InnerHtml += mainDiv.ToString();

            //outerDiv.InnerHtml += innerDiv.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }


        private static string GetGridHeader<TModel>(List<IGridColumn<TModel>> columns, string headerStyle) where TModel : class
        {
            string rv = "";
            TagBuilder tr = new TagBuilder("tr");
            tr.MergeAttribute("class", headerStyle);

            //ѭ����ӱ�ͷ

            int maxLevel = columns.Max(x => x.MaxLevel);
            foreach (var col in columns)
            {
                TagBuilder th = new TagBuilder("th");
                th.MergeAttribute("scope", "col");
                th.InnerHtml = col.Header;
                th.MergeAttribute("rowspan", (maxLevel - col.MaxLevel + 1).ToString());
                th.MergeAttribute("colspan", col.MaxChildrenCount.ToString());
                if (col.MaxChildrenCount > 1)
                {
                    th.MergeAttribute("style", "text-align:center");
                }
                tr.InnerHtml += th.ToString();
            }
            rv += tr.ToString();
            //foreach (var col in columns)
            //{
            //    if (col.Children != null && col.Children.Count > 0)
            //    {
            //        rv += GetGridHeader<TModel>(col.Children, headerStyle);
            //    }
            //}
            List<IGridColumn<TModel>> AllChildren = new List<IGridColumn<TModel>>();
            foreach (var col in columns)
            {
                if (col.Children != null && col.Children.Count > 0)
                {
                    AllChildren.AddRange(col.Children);
                }
            }
            if (AllChildren.Count > 0)
            {
                rv += GetGridHeader<TModel>(AllChildren, headerStyle);
            }
            return rv;
        }

        /// <summary>
        /// ���������б�ؼ�
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel</typeparam>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="entityList">�����б�</param>
        /// <param name="columns">Ҫ��ʾ����</param>
        /// <param name="treeColumn">ָ��ĳ���ֶ���ʾ����</param>
        /// <returns></returns>
        public static MvcHtmlString LS_TreeGrid<TViewModel, TModel>(
            this HtmlHelper<TViewModel> html,
            List<TModel> entityList,
            List<IGridColumn<TModel>> columns,
            IGridColumn<TModel> treeColumn)
            where TViewModel : IPagedList
            where TModel : ITreeData<TModel>
        {
            //������ת��ΪTreeGrid����Ҫ��Json��ʽ
            string colString = "";
            foreach (var col in columns)
            {
                colString += "{field:'" + col.ColumnName + "',title:'" + col.Header + "'";
                if (col.Width.HasValue)
                {
                    colString += ",width:" + col.Width.Value;
                }
                colString += "},";
            }
            if (colString.EndsWith(","))
            {
                colString = colString.Remove(colString.Length - 1);
            }
            string data = "";
            foreach (var entity in entityList.Where(x => x.Parent == null))
            {
                data += GetTreeNodeString(entity, columns) + ",";
            }
            if (data.EndsWith(","))
            {
                data = data.Remove(data.Length - 1);
            }

            TagBuilder mainDiv = new TagBuilder("div");

            TagBuilder table = new TagBuilder("table");
            table.MergeAttribute("id", "pagefunctiontable");

            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml += @"	$(document).ready(function(){
		       var tg =	$('#pagefunctiontable').treegrid({
				    nowrap: true,
                    idField: '_id',
				    treeField:'" + treeColumn.ColumnName + @"',
				    columns:[[
					    " + colString + @"
				    ]]
			    });
                var data = [" + data + @"];
                $('#pagefunctiontable').treegrid('append', {
				    data: data
			    });
		    });";

            mainDiv.InnerHtml += table.ToString();
            mainDiv.InnerHtml += script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }

        /// <summary>
        /// ��ȡĳ�����ݵ�TreeGrid���ݸ�ʽ
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="entity">Model��</param>
        /// <param name="columns">Ҫ��ʾ����</param>
        /// <returns>���ɵ�HTML</returns>
        private static string GetTreeNodeString<TModel>(TModel entity, List<IGridColumn<TModel>> columns) where TModel : ITreeData<TModel>
        {
            string data = "";
            data += "{";
            GridColumn<BasePoco> IdColumn = new GridColumn<BasePoco> { ColumnNameExp = x => x.ID };
            data += "_id:" + IdColumn.GetText(entity as BasePoco) + ",";
            foreach (var col in columns)
            {
                data += col.ColumnName + ":'" + col.GetText(entity) + "',";
            }
            if (entity.Children != null && entity.Children.Count > 0)
            {
                data += "children:[";
                foreach (var child in entity.Children)
                {
                    data += GetTreeNodeString(child, columns) + ",";
                }
                if (data.EndsWith(","))
                {
                    data = data.Remove(data.Length - 1);
                }
                data += "]";
            }
            else
            {
                if (data.EndsWith(","))
                {
                    data = data.Remove(data.Length - 1);
                }
            }
            data += "}";
            return data;
        }

        #region ѡ��Ա��
        /// <summary>
        /// ͨ������IDѡ��Ա����ѡ��Ա�����صĸ�ʽ��ID|UserCode|UserName��
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="listID">��Ų���ID�Ŀؼ�</param>
        /// <param name="DateID">��ȡ��ʱ��ؼ���Ӧ��ID�����û�иÿؼ�����Ĭ���б�ǰ��ְ��Ա��������ͨ���ÿؼ���ʱ���ȡ���ʱ����ְ��Ա��</param>
        /// <param name="ReturnScriptFunction">���ڽ���ѡ��Ա������ֵ�ķ���,Ĭ�Ϸ���ΪFF_SelectUserFinish</param>
        /// <returns>���ɵ�HTML</returns>
        public static MvcHtmlString LS_SelectUser<TViewModel>(this HtmlHelper<TViewModel> html, string listID, string ReturnScriptFunction = "FF_SelectUserFinish", string DateID = "")
        {
            string datestr = ""; //�洢���ڿؼ���Ϣ��
            if (!string.IsNullOrEmpty(DateID))
            {
                datestr = @"var date=$('#" + DateID + @"').val();                            
                            if(date==undefined||date==''){
                            alert('" + Resources.Language.��ѡ��ʱ�� + @"');return;
                            }
                            var datestr='&date='+date;";

            }
            else
            {
                datestr = "var datestr='';";
            }
            TagBuilder mainDiv = new TagBuilder("div");
            TagBuilder button = new TagBuilder("input");
            button.MergeAttribute("type", "button");
            button.MergeAttribute("id", "OCF_SelectUserByDept");
            button.MergeAttribute("style", "min-width: 80px");
            button.MergeAttribute("value", Resources.Language.ѡ��Ա��);
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"$(document).ready(function () {
            $('#OCF_SelectUserByDept').click(function () {
                var deptid=$('#" + listID + @"').val();
                if(isNaN(deptid)||deptid==''){
                    alert('" + Resources.Language.��ѡ�� + " " + Resources.Language.���� + @"');
                    return;
                }
                " + datestr +
                @"var newDiv = $(document.createElement('div'));
                newDiv.dialog({
                    autoOpen: true,
                    width: 650,
                    height: 500,
                    resizable: true,
                    title: '" + Resources.Language.ѡ��Ա�� + @"',
                    modal: true,
                    open: function (event, ui) { 
                        $(this).load('/OCF/DepartmentUserIndex?t='+event.timeStamp+'&deptid='+deptid+datestr);
                        $(':input:first').focus();
                    },
                    buttons: [{
                        text: '" + Resources.Language.ȷ�� + @"',
                        click: function () {
                            if (typeof " + ReturnScriptFunction + @" == 'function') {
                                var str = """";
                                $(':input[data-UserID]').each(function () {
                                    if ($(this).attr('checked') == true) {
                                        str = $(this).attr('data-UserID') + ';' + str;
                                    }
                                });
                                if (str.length > 0) {
                                    str = str.substring(0, str.length - 1);
                                }
                                " + ReturnScriptFunction + @"(str);
                            }
                            $(this).dialog('close');
                        }
                    },
					{
					    text: '" + Resources.Language.ȡ�� + @"',
					    click: function ()
					    { $(this).dialog('close'); }
					}],
                    close: function(event, ui) { newDiv.remove(); }
                });
                return false;
            });

        });";
            mainDiv.InnerHtml += button.ToString();
            mainDiv.InnerHtml += script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }

        /// <summary>
        /// ѡ���û��ؼ���ѡ��Ա�����صĸ�ʽ��ID|UserCode|UserName��
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="Type">1ΪOCF��2ΪEmployee��3OCFΪEmployee</param>
        /// <param name="Text">��ť��ʾ���ı�</param>
        /// <param name="id">�ð�ť��ID</param>
        /// <param name="returnFunction">���ȷ���󷵻صķ�������</param>
        /// <returns>���ɵ�HTML</returns>
        [Obsolete("�ѹ�ʱ", false)]
        public static MvcHtmlString LS_SelectUser<TViewModel>(this HtmlHelper<TViewModel> html, string Type, string Text, string id, string returnFunction)
        {
            TagBuilder mainDiv = new TagBuilder("div");
            TagBuilder button = new TagBuilder("button");
            button.MergeAttribute("id", "OCF_SelectUser_" + id);
            button.MergeAttribute("style", "min-width: 80px");
            button.MergeAttribute("class", "btn_bg");
            button.InnerHtml = Text;
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"$(document).ready(function () {
            $('#OCF_SelectUser_" + id + @"').click(function () {
                var newDiv = $(document.createElement('div'));
                newDiv.dialog({
                    autoOpen: true,
                    width: 650,
                    height: 500,
                    resizable: true,
                    title: '" + Text + @"',
                    modal: true,
                    open: function (event, ui) { 
                        $(this).load('/OCF/UserIndex?t='+event.timeStamp+'&type=" + Type + @"');
                        $(':input:first').focus();
                    },
                    buttons: [{
                        text: '" + Resources.Language.ȷ�� + @"',
                        click: function () {
                            if (typeof " + returnFunction + @" == 'function') {
                                var str = """";
                                $(':input[data-OCFUserID]').each(function () {
                                    if ($(this).attr('checked') == true) {
                                        str = $(this).attr('data-OCFUserID') + ';' + str;
                                    }
                                });
                                if (str.length > 0) {
                                    str = str.substring(0, str.length - 1);
                                }
                                " + returnFunction + @"(str);
                            }
                            $(this).dialog('close');
                        }
                    },
					{
					    text: '" + Resources.Language.ȡ�� + @"',
					    click: function ()
					    { $(this).dialog('close'); }
					}],
                close: function(event, ui) { newDiv.remove(); }
                });
                return false;
            });

        });";
            mainDiv.InnerHtml += button.ToString();
            mainDiv.InnerHtml += script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }
        /// <summary>
        /// ��ѡ���û��ؼ���ѡ��Ա�����صĸ�ʽ��ID|UserCode|UserName��
        /// </summary>
        /// <typeparam name="TModel">Model</typeparam>
        /// <param name="IsSingle">�Ƿ��ǵ�ѡԱ��,Ĭ�ϲ��ǵ�ѡ</param>
        /// <param name="DateID">��ȡ��ʱ��ؼ���Ӧ��ID�����û�иÿؼ�����Ĭ���б�ǰ��ְ��Ա��������ͨ���ÿؼ���ʱ���ȡ���ʱ����ְ��Ա��</param>
        /// <param name="ReturnScriptFunction">���ڽ���ѡ��Ա������ֵ�ķ���,Ĭ�Ϸ���ΪFF_SelectUserFinish</param>
        /// <returns>���ɵ�HTML</returns>
        public static MvcHtmlString LS_SelectUser<TViewModel>(this HtmlHelper<TViewModel> html, bool IsSingle = false, string ReturnScriptFunction = "FF_SelectUserFinish", string DateID = "")
        {
            string datestr = "";
            if (!string.IsNullOrEmpty(DateID))
            {
                datestr = @"var date=$('#" + DateID + @"').val();                            
                            if(date==undefined||date==''){
                            alert('" + Resources.Language.��ѡ��ʱ�� + @"');return;
                            }
                            var datestr='&date='+date;";

            }
            else
            {
                datestr = "var datestr='';";
            }
            TagBuilder mainDiv = new TagBuilder("div");
            string id = "OCF_SelectUser_" + DateTime.Now.Ticks.ToString();
            TagBuilder button = new TagBuilder("button");
            button.MergeAttribute("id", id);
            button.MergeAttribute("style", "min-width: 80px");
            //button.MergeAttribute("class", "btn_bg");
            button.InnerHtml = Resources.Language.ѡ��Ա��;
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"$(document).ready(function () {
            $('#" + id + @"').click(function () {
                " + datestr +
                @"var newDiv = $(document.createElement('div'));
                newDiv.dialog({
                    autoOpen: true,
                    width: 650,
                    height: 500,
                    resizable: true,
                    title: '" + Resources.Language.��ѡ��Ա�� + @"',
                    modal: true,
                    open: function (event, ui) { 
                        $(this).load('/OCF/SelectEmployees?t='+event.timeStamp+'&IsSingle=" + IsSingle + @"'+datestr);
                        $(':input:first').focus();
                    },
                    buttons: [{
                        text: '" + Resources.Language.ȷ�� + @"',
                        click: function () {
                            if (typeof " + ReturnScriptFunction + @" == 'function') {
                                var str = """";
                                $(':input[data-OCFUserID]').each(function () {
                                    if ($(this).attr('checked') == true) {
                                        str = $(this).attr('data-OCFUserID') + ';' + str;
                                    }
                                });
                                if (str.length > 0) {
                                    str = str.substring(0, str.length - 1);
                                }
                                " + ReturnScriptFunction + @"(str);
                            }
                            $(this).dialog('close');
                        }
                    },
					{
					    text: '" + Resources.Language.ȡ�� + @"',
					    click: function ()
					    { $(this).dialog('close'); }
					}],
                    close: function(event, ui) { newDiv.remove(); }
                });
                return false;
            });

        });";
            mainDiv.InnerHtml += button.ToString();
            mainDiv.InnerHtml += script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }
        #endregion


        #region ѡ����

        /// <summary>
        /// ѡ�񵥸����� Add by dufei 2011-6-21
        /// </summary>
        /// <typeparam name="TViewModel">Model</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="Text">��Ҫ��ʾ��ť�ı�ֵ</param>
        /// <returns></returns>
        public static MvcHtmlString LS_SelectDepartment<TViewModel>(this HtmlHelper<TViewModel> html, string ReturnScriptFunction = "FF_SelectDepartmentFinish")
        {
            TagBuilder mainDiv = new TagBuilder("div");
            TagBuilder button = new TagBuilder("button");
            string id = "SelectDepartment_" + DateTime.Now.Ticks.ToString();
            button.MergeAttribute("id", id);
            button.MergeAttribute("style", "min-width: 80px");
            //button.MergeAttribute("class", "btn_bg");
            button.InnerHtml = Resources.Language.ѡ����;
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"$(document).ready(function () {
            $('#" + id + @"').click(function () {
                var newDiv = $(document.createElement('div'));
                newDiv.dialog({
                    autoOpen: true,
                    width: 650,
                    height: 500,
                    resizable: true,
                    title: '" + Resources.Language.ѡ���� + @"',
                    modal: true,
                    open: function (event, ui) {
                        $(this).load('/OCF/SelectDepartment?t='+event.timeStamp);
                        $(':input:first').focus();
                    },
                    buttons: [{
                        text: '" + Resources.Language.ȷ�� + @"',
                        click: function () {
                            if (typeof " + ReturnScriptFunction + @" == 'function') {
                                var str = """";
                                $(':input[data-Department]').each(function () {
                                    if ($(this).attr('checked') == true) {
                                        str = $(this).attr('data-Department') + ';' + str;
                                    }
                                });
                                    " + ReturnScriptFunction + @"(str);
                            }
                            $(this).dialog('close');
                        }
                    },
					{
					    text: '" + Resources.Language.ȡ�� + @"',
					    click: function ()
					    { $(this).dialog('close'); }
					}],
                close: function(event, ui) { newDiv.remove(); }
                });
                return false;
            });

        });";
            mainDiv.InnerHtml += button.ToString();
            mainDiv.InnerHtml += script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }

        #endregion

        #region ѡ�񵥸���Ƹ���󵥺�

        /// <summary>
        /// ѡ�񵥸����� Add by zhaohl 2011-6-23
        /// </summary>
        /// <typeparam name="TViewModel">Model</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="Text">��Ҫ��ʾ��ť�ı�ֵ</param>
        /// <returns></returns>
        public static MvcHtmlString LS_SelectRecruitment<TViewModel>(this HtmlHelper<TViewModel> html, string Text)
        {
            TagBuilder mainDiv = new TagBuilder("span");
            TagBuilder button = new TagBuilder("img");
            button.MergeAttribute("style", "cursor:pointer");
            button.MergeAttribute("tabIndex", "-1");
            button.MergeAttribute("id", "SelectRecruitment");
            button.MergeAttribute("src", "../images/dept.gif");
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"$(document).ready(function () {
            $('#SelectRecruitment').click(function () {
                var newDiv = $(document.createElement('div'));
                newDiv.dialog({
                    autoOpen: true,
                    width: 800,
                    height: 600,
                    resizable: true,
                    title: '" + Text + @"',
                    modal: true,
                    open: function (event, ui) {
                        $(this).load('/HRRecruitment/TrailRecruitmentIndex?t='+event.timeStamp);
                        $(':input:first').focus();
                    },
                    buttons: [{
                        text: '" + Resources.Language.ȷ�� + @"',
                        click: function () {
                            if (typeof FF_SelectFF_RecruitmentFinish == 'function') {
                                var str = """";
                                $(':input[data_HRRecruitmentID]').each(function () {
                                    if ($(this).attr('checked') == true) {
                                        str = $(this).attr('data_HRRecruitmentID');
                                    }
                                });
                                    FF_SelectFF_RecruitmentFinish(str);
                            }
                            $(this).dialog('close');
                        }
                    },
					{
					    text: '" + Resources.Language.ȡ�� + @"',
					    click: function ()
					    { $(this).dialog('close'); 
                            newDiv.children().remove();
                        }
					}],
                    close: function (event, ui) {
                    newDiv.children().remove();
                }

                });
                return false;
            });

        });";
            mainDiv.InnerHtml += button.ToString();
            mainDiv.InnerHtml += script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }

        #endregion

        #region ��ѯ��ְ��Ϣ�п��������ְ��̸��¼������
        /// <summary>
        /// ѡ�񵥸���ְ��Ϣ Add zhaohl 2011-7-12
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="html"></param>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static MvcHtmlString LS_SelectDimission<TViewModel>(this HtmlHelper<TViewModel> html, string Text)
        {
            TagBuilder mainDiv = new TagBuilder("div");
            TagBuilder button = new TagBuilder("button");
            button.MergeAttribute("id", "SelectDimission");
            button.MergeAttribute("style", "min-width: 80px");
            button.InnerHtml = Text;
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"$(document).ready(function () {
            $('#SelectDimission').click(function () {
                var newDiv = $(document.createElement('div'));
                newDiv.dialog({
                    autoOpen: true,
                    width: 800,
                    height: 600,
                    resizable: true,
                    title: '" + Text + @"',
                    modal: true,
                    open: function (event, ui) {
                        $(this).load('/Dismission/DismissionIndex?t='+event.timeStamp);
                        $(':input:first').focus();
                    },
                    buttons: [{
                        text: '" + Resources.Language.ȷ�� + @"',
                        click: function () {
                            if (typeof FF_SelectFF_DismissionFinish == 'function') {
                                var str = """";
                                $(':input[data_DismissionID]').each(function () {
                                    if ($(this).attr('checked') == true) {
                                        str = $(this).attr('data_DismissionID');
                                    }
                                });
                                    FF_SelectFF_DismissionFinish(str);
                            }
                            $(this).dialog('close');
                        }
                    },
					{
					    text: '" + Resources.Language.ȡ�� + @"',
					    click: function ()
					    { $(this).dialog('close');
                            newDiv.children().remove();
                            }
					}],
                    close: function (event, ui) {
                    newDiv.children().remove();
                }

                });
                return false;
            });

        });";
            mainDiv.InnerHtml += button.ToString();
            mainDiv.InnerHtml += script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }
        #endregion


        /// <summary>
        /// ����һ���ؼ��Ŀ��
        /// </summary>
        /// <typeparam name="TModel">ViewModel</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="property">��ȡ�ֶεı��ʽ</param>
        /// <param name="width">Ҫ�趨�Ŀ��</param>
        /// <returns></returns>
        public static MvcHtmlString LS_SetInputWidth<TViewModel>(this HtmlHelper<TViewModel> html, Expression<Func<TViewModel, object>> property, int width)
        {
            string InputID = UtilsTool.GetPropertyName(property).Replace(".", "_");
            return MvcHtmlString.Create("$(\"#" + InputID + "\").width(" + width + ");");
        }

        /// <summary>
        /// ���˵��ؼ�
        /// </summary>
        /// <typeparam name="TViewModel">Model</typeparam>
        /// <param name="html">HtmlHelper��</param>
        /// <param name="pages">����ҳ���б�</param>
        /// <returns>���ɵ�Html</returns>
        public static MvcHtmlString LS_Menu<TViewModel>(this HtmlHelper<TViewModel> html, List<PageFunction> pages)
        {
            string rv = "";
            int treelevel = 0;
            foreach (var page in pages)
            {
                TagBuilder li = new TagBuilder("li");
                TagBuilder a = new TagBuilder("a");

                a.MergeAttribute("href", "#");
                if (page.Children == null || page.Children.Count == 0)
                {
                    a.MergeAttribute("onclick", UtilsTool.GetLoadPageScript(page.Url));
                }

                if (page.Parent == null)
                {
                    treelevel++;
                    a.MergeAttribute("name", treelevel.ToString());
                    TagBuilder span = new TagBuilder("span");
                    span.MergeAttribute("style", @"background:url(../../images/icon_sub_factory.png) no-repeat 2px 2px ;width:18px;height:18px;margin-right:5px;");
                    a.InnerHtml += span.ToString();
                    a.MergeAttribute("onclick", "javascript:setTreeLevel(" + treelevel.ToString() + ");");
                }
                else
                {
                    //������Ӳ˵����Li��ǩ�����HTMLԪ��
                    TagBuilder img = new TagBuilder("img");
                    img.MergeAttribute("src", "../../images/pkg.gif");
                    img.MergeAttribute("width", "16px");
                    img.MergeAttribute("heigh", "16px");
                    a.InnerHtml += img.ToString();
                    a.MergeAttribute("class", "space");
                }
                a.InnerHtml += page.GetLanguageSpecificContent(x => x.FunctionName);
                li.InnerHtml += a.ToString();
                if (page.Children != null && page.Children.Count > 0)
                {

                    TagBuilder ul = new TagBuilder("ul");
                    ul.InnerHtml = LS_Menu<TViewModel>(html, page.Children).ToHtmlString();
                    li.InnerHtml += ul.ToString();
                }


                rv += li.ToString();
            }
            return MvcHtmlString.Create(rv);
        }


        //�����ˣ���־ǿ������ʱ�䣺6-25
        /// <summary>
        /// ��ʼ��ʵ���е�Ԥ���ֶΣ�IsValid,Creator,CreateDate,Modifier,ModifyDate,Memo
        /// </summary>
        /// <param name="sp">Ҫ�޸ĵ�Model</param>
        /// <param name="ocf">��ǰOCF�û�(FFUser)</param>
        /// <param name="isvalid">�Ƿ���Ч����ӦIsValid�ֶ�</param>        
        public static void SetDLMSPoco(BaseDLMSPoco sp, OCFUser ocf, bool isvalid = true)
        {
            sp.IsValid = isvalid;
            sp.Creator = ocf.ITCode;
            sp.CreateDate = DateTime.Now;
            sp.Modifier = ocf.ITCode;
            sp.ModifyDate = DateTime.Now;
            sp.Memo = "gogoc";
        }

        //�����ˣ�Ф�ϣ�����ʱ�䣺2011-07-01
        /// <summary>
        /// ��λ��������ѡ��
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="html"></param>
        /// <param name="id">�ؼ�ID</param>
        /// <param name="name">�ؼ�Name</param>
        /// <param name="onBlur">ѡ���뿪ʱ�������¼�</param>
        /// <returns></returns>
        public static MvcHtmlString LS_DropDownList<TViewModel>(this HtmlHelper<TViewModel> html, string id, string name, string onBlur)
        {
            TagBuilder mainDiv = new TagBuilder("div");
            mainDiv.MergeAttribute("id", id);
            TagBuilder script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml = @"var combo = new dhtmlXCombo('" + id + "', '" + name + "', 150);combo.enableFilteringMode(true, '/OCF/LoadJob',false);combo.attachEvent('onOpen',function(){this.setComboValue('');if (this.optionsArr.length == 0 || this.getComboText() == null || this.getComboText() == '') {this.loadXML('/OCF/LoadJob',false)}});combo.attachEvent('onBlur',function(){ var v = this.getSelectedValue();if(v==null){this.setComboValue('');}try{" + onBlur + "(v);}catch(e){}});";
            mainDiv.InnerHtml = script.ToString();
            return MvcHtmlString.Create(mainDiv.ToString());
        }
    }
}