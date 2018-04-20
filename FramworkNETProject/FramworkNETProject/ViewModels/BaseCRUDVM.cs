using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SupportClasses;
using Models;
using Models.Attributes;
using Controllers;
using Helpers;
using DataAccess.SqlServer;

namespace ViewModels
{
    public class BaseCRUDVM<TModel> : BaseVM, IValidatableObject where TModel : BasePoco
    {

        public TModel Entity { get; set; }

        private DuplicatedInfo<TModel> _duplicatedPropertiesCheck { get; set; }
        private List<Expression<Func<TModel, object>>> _toInclude { get; set; }
        public BaseCRUDVM()
        {
            var ctor = typeof(TModel).GetConstructor(Type.EmptyTypes);
            Entity = ctor.Invoke(null) as TModel;
            var lists = typeof(TModel).GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>));
            foreach (var li in lists)
            {
                var gs = li.PropertyType.GetGenericArguments();
                var newObj = Activator.CreateInstance( typeof(List<>).MakeGenericType(gs[0]));
                li.SetValue(Entity,newObj,null);
            }
            this._duplicatedPropertiesCheck = SetDuplicatedCheck();
        }

        public virtual DuplicatedInfo<TModel> SetDuplicatedCheck()
        {
            return null;
        }

        protected void SetInclude(params Expression<Func<TModel, object>>[] exps)
        {
            _toInclude = new List<Expression<Func<TModel, object>>>();
            _toInclude.AddRange(exps);
        }

        public virtual TModel GetByID(long Id)
        {
            TModel rv = null;
            var query = DC.GetSet<TModel>().AsQueryable();
            if (typeof(TModel).GetInterface("IMLData") != null)
            {
                query = query.SInclude("MLContents");
            }
            if (_toInclude != null)
            {
                foreach (var item in _toInclude)
                {
                    query = query.Include(item);
                }
            }
            rv = query.Where(x => x.ID == Id).SingleOrDefault();
            return rv;
        }

        public virtual List<TModel> GetAll()
        {
            List<TModel> rv = new List<TModel>();
            var query = DC.GetSet<TModel>().AsQueryable();
            if (typeof(TModel).GetInterface("IMLData") != null)
            {
                query = DC.GetSet<TModel>().SInclude("MLContents");
            }
            if (_toInclude != null)
            {
                foreach (var item in _toInclude)
                {
                    query = query.Include(item);
                }
            }
            rv = query.ToList();
            return rv;
        }

        public virtual void DoAdd()
        {
            DC.GetSet<TModel>().Add(Entity);
            DC.Commit();
        }

        public virtual void DoEdit()
        {
            if (typeof(TModel).GetInterface("IMLData") != null)
            {
                if ((Entity as dynamic).MLContents != null)
                {
                    foreach (var item in (Entity as dynamic).MLContents)
                    {
                        DC.SetStatus(item, System.Data.Entity.EntityState.Modified);
                    }
                }
            }
            DC.SetStatus(Entity, System.Data.Entity.EntityState.Modified);
            DC.Commit();
        }

        public virtual void DoDelete()
        {
            DC.DeleteEntity(DC.GetSet<TModel>(),Entity);
            DC.Commit();
        }

        protected DuplicatedInfo<TModel> CreateFieldsInfo(params DuplicatedField<TModel>[] FieldExps)
        {
            return DuplicatedInfo<TModel>.CreateFieldInfo(FieldExps);
        }

        public static DuplicatedField<TModel> SimpleField(Expression<Func<TModel, object>> FieldExp)
        {
            return DuplicatedField<TModel>.SimpleField(FieldExp);
        }

        public static DuplicatedField<TModel> SubField<V>(Expression<Func<TModel, List<V>>> MiddleExp, params Expression<Func<V, object>>[] FieldExps)
        {
            return ComplexDuplicatedField<TModel, V>.SubField(MiddleExp, FieldExps);
        }


        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> rv = new List<ValidationResult>();
            ValidateMLData(rv);
            ValidateDuplicateData(rv);
            return rv;
        }

        protected void ValidateDuplicateData(List<ValidationResult> rv)
        {
            if (this._duplicatedPropertiesCheck != null && this._duplicatedPropertiesCheck.Groups.Count > 0)
            {
                var baseExp = DC.GetSet<TModel>().AsQueryable();
                var modelType = typeof(TModel);
                ParameterExpression para = Expression.Parameter(modelType, "tm");
                foreach (var group in this._duplicatedPropertiesCheck.Groups)
                {
                    List<Expression> conditions = new List<Expression>();
                    //生成一个表达式，类似于 x=>x.ID != id
                    MemberExpression idLeft = Expression.Property(para, "ID");
                    ConstantExpression idRight = Expression.Constant(Entity.ID);
                    BinaryExpression idNotEqual = Expression.NotEqual(idLeft, idRight);
                    conditions.Add(idNotEqual);
                    List<PropertyInfo> props = new List<PropertyInfo>();
                    foreach (var field in group.Fields)
                    {
                        if (field.DirectFieldExp != null)
                        {
                            var item = field.DirectFieldExp;
                            string propertyName = UtilsTool.GetPropertyName(item);
                            var prop = UtilsTool.GetPropertyInfo(item);
                            props.Add(prop);
                            var func = item.Compile();
                            var val = func.Invoke(this.Entity);

                            if (val == null)
                            {
                                continue;
                            }
                            if (val is string && val.ToString() == "")
                            {
                                var requiredAttrs = prop.GetCustomAttributes(typeof(RequiredAttribute), false);

                                if (requiredAttrs == null || requiredAttrs.Length == 0)
                                {
                                    continue;
                                }
                                else
                                {
                                    var requiredAtt = requiredAttrs[0] as RequiredAttribute;
                                    if (requiredAtt.AllowEmptyStrings == true)
                                    {
                                        continue;
                                    }
                                }
                            }
                            //生成一个表达式，类似于 x=>x.field == val
                            Expression left = Expression.Property(para, propertyName);
                            if (left.Type.IsGenericType && left.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                left = Expression.Property(left, "Value");
                            }
                            if (left.Type == typeof(string))
                            {
                                left = Expression.Call(left, typeof(String).GetMethod("Trim", Type.EmptyTypes));
                            }
                            if (val is string)
                            {
                                val = val.ToString().Trim();
                            }
                            ConstantExpression right = Expression.Constant(val);
                            BinaryExpression equal = Expression.Equal(left, right);
                            conditions.Add(equal);
                        }
                        else
                        {
                            dynamic dField = field;
                            Expression exp = dField.GetExpression(Entity, para);
                            if (exp != null)
                            {
                                conditions.Add(exp);
                            }
                            props.AddRange(dField.GetProperties());
                        }
                    }

                    int count = 0;
                    if (conditions.Count > 1)
                    {
                        Expression conExp = conditions[0];
                        for (int i = 1; i < conditions.Count; i++)
                        {
                            conExp = Expression.And(conExp, conditions[i]);
                        }

                        MethodCallExpression whereCallExpression = Expression.Call(
                             typeof(Queryable),
                             "Where",
                             new Type[] { modelType },
                             baseExp.Expression,
                             Expression.Lambda<Func<TModel, bool>>(conExp, new ParameterExpression[] { para }));
                        var result = baseExp.Provider.CreateQuery(whereCallExpression);
                        foreach (var res in result)
                        {
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        string AllName = "";
                        foreach (var prop in props)
                        {
                            var dispAttrs = prop.GetCustomAttributes(typeof(DisplayAttribute), false);
                            string name = prop.Name;
                            if (name == "LanguageCode")
                            {
                                continue;
                            }
                            if (dispAttrs != null && dispAttrs.Length > 0)
                            {
                                var attr = dispAttrs[0] as DisplayAttribute;
                                name = attr.Name;
                                if (attr.ResourceType != null)
                                {
                                    name = attr.ResourceType.GetProperty(name).GetValue(null, null).ToString();
                                }
                            }
                            AllName += name + ",";
                        }
                        if (AllName.EndsWith(","))
                        {
                            AllName = AllName.Remove(AllName.Length - 1);
                        }
                        if (props.Count == 1)
                        {
                            rv.Add(new ValidationResult(AllName + Resources.Language.字段重复, GetValidationFieldName(props[0])));
                        }
                        else if (props.Count > 1)
                        {
                            rv.Add(new ValidationResult(AllName + Resources.Language.组合字段重复, GetValidationFieldName(props[0])));
                        }
                        else
                        {
                            rv.Add(new ValidationResult(AllName + Resources.Language.组合字段重复));
                        }
                    }
                }
            }
        }

        protected string[] GetValidationFieldName(PropertyInfo pi)
        {
            if (pi.DeclaringType.IsSubclassOf(typeof(MLContent)))
            {
                return new[] { "Entity.MLContents" + pi.Name };
            }
            else
            {
                return new[] { "Entity." + pi.Name };
            }
        }

        protected void ValidateMLData(List<ValidationResult> rv)
        {
            Dictionary<string, int> counter = new Dictionary<string, int>();
            Dictionary<string, int> required = new Dictionary<string, int>();
            Dictionary<string, string> lengthError = new Dictionary<string, string>();
            if (Entity is IMLData)
            {
                dynamic dEntity = Entity;
                if (dEntity.MLContents != null)
                {
                    foreach (var lan in BaseController.Languages)
                    {
                        MLContent found = null;
                        foreach (var ml in dEntity.MLContents)
                        {
                            if (ml.LanguageCode == lan.LanguageCode)
                            {
                                found = ml;
                            }
                        }

                        var mltype = (dEntity.MLContents.GetType() as Type).GetGenericArguments()[0];
                        var pros = mltype.GetProperties();
                        foreach (var pro in pros)
                        {
                            if (pro.PropertyType == typeof(string))
                            {
                                if (!counter.ContainsKey(pro.Name))
                                {
                                    counter[pro.Name] = 0;
                                }
                                if (!required.ContainsKey(pro.Name))
                                {
                                    object[] attrs = pro.GetCustomAttributes(typeof(MLRequiredAttribute), false);
                                    if (attrs.Length > 0)
                                    {
                                        MLRequiredAttribute attr = attrs[0] as MLRequiredAttribute;
                                        if (attr.AllLanguageRequired == true)
                                        {
                                            required[pro.Name] = 2;
                                        }
                                        else
                                        {
                                            required[pro.Name] = 1;
                                        }
                                    }
                                    else
                                    {
                                        required[pro.Name] = 0;
                                    }
                                }

                                if (found != null)
                                {
                                    object[] attrs = pro.GetCustomAttributes(typeof(StringLengthAttribute), false);
                                    if (attrs.Length > 0)
                                    {
                                        StringLengthAttribute attr = attrs[0] as StringLengthAttribute;
                                        object val = pro.GetValue(found, null);
                                        if (val is string && val != null && val.ToString().Length > attr.MaximumLength)
                                        {
                                            lengthError[pro.Name] = attr.FormatErrorMessage(null);
                                        }
                                    }
                                }
                                object v = null;
                                if (found != null)
                                {

                                    v = pro.GetValue(found, null);
                                }
                                if (v != null && v.ToString().Trim() != "")
                                {
                                    counter[pro.Name] = counter[pro.Name] + 1;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var item in lengthError)
            {
                rv.Add(new ValidationResult(item.Value, new[] { "Entity.MLContents" + item.Key }));
            }

            foreach (var item in required)
            {
                if (item.Value == 1 && counter[item.Key] == 0)
                {
                    rv.Add(new ValidationResult(Resources.Language.多语言_一种, new[] { "Entity.MLContents" + item.Key }));
                }
                else
                {
                    if (counter[item.Key] < item.Value)
                    {
                        rv.Add(new ValidationResult(Resources.Language.多语言_全部, new[] { "Entity.MLContents" + item.Key }));
                    }
                }
            }
        }

        //public void WriteLogs(DateTime Date, string ApplyCode, string Posison, string path)
        //{
        //    #region 写txt日志文件
        //    string msg = "单据编号:" + ApplyCode + "-->时间:" + DateTime.Now.ToString() + "-->所处位置:" + Posison;
        //    System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Append);
        //    System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.Default);
        //    sw.WriteLine(msg);
        //    sw.Close();
        //    fs.Close();
        //    #endregion
        //}
    }
}