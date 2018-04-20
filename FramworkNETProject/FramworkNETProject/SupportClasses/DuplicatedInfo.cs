using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Helpers;

namespace SupportClasses
{
    public class DuplicatedInfo<T>
    {
        public List<DuplicatedGroup<T>> Groups { get; set; }

        public static DuplicatedInfo<T> CreateFieldInfo(params DuplicatedField<T>[] FieldExps)
        {
            DuplicatedInfo<T> rv = new DuplicatedInfo<T>();
            rv.Groups = new List<DuplicatedGroup<T>>();
            rv.Groups.Add(new DuplicatedGroup<T>());
            rv.Groups[0].Fields = new List<DuplicatedField<T>>();
            foreach (var exp in FieldExps)
            {
                rv.Groups[0].Fields.Add(exp);
            }
            return rv;
        }

        public void AddGroup(params DuplicatedField<T>[] FieldExps)
        {
            DuplicatedGroup<T> newGroup = new DuplicatedGroup<T>();
            newGroup.Fields = new List<DuplicatedField<T>>();
            foreach (var exp in FieldExps)
            {
                newGroup.Fields.Add(exp);
            }
            Groups.Add(newGroup);
        }
    }

    public class DuplicatedGroup<T>
    {
        public List<DuplicatedField<T>> Fields { get; set; }
    }

    public class DuplicatedField<T> 
    {
        public Expression<Func<T, object>> DirectFieldExp { get; set; }

        public static DuplicatedField<T> SimpleField(Expression<Func<T, object>> FieldExp)
        {
            DuplicatedField<T> rv = new DuplicatedField<T>();
            rv.DirectFieldExp = FieldExp;
            return rv;
        }

    }

    public class ComplexDuplicatedField<T, V> : DuplicatedField<T>
    {
        public Expression<Func<T, List<V>>> MiddleExp { get; set; }
        public List<Expression<Func<V, object>>> SubFieldExps { get; set; }

        public static ComplexDuplicatedField<T, V> SubField(Expression<Func<T, List<V>>> MiddleExp, params Expression<Func<V, object>>[] FieldExps)
        {
            ComplexDuplicatedField<T, V> rv = new ComplexDuplicatedField<T, V>();
            rv.MiddleExp = MiddleExp;
            rv.SubFieldExps = new List<Expression<Func<V, object>>>();
            rv.SubFieldExps.AddRange(FieldExps);
            return rv;
        }

        public Expression GetExpression(T Entity, ParameterExpression para)
        {
            ParameterExpression midPara = Expression.Parameter(typeof(V), "tm2");
            var list = MiddleExp.Compile().Invoke(Entity);

            List<Expression> allExp = new List<Expression>();
            Expression rv = null;
            foreach (var li in list)
            {
                List<Expression> innerExp = new List<Expression>();
                bool needBreak = false;
                foreach (var SubFieldExp in SubFieldExps)
                {
                    Expression left = Expression.Property(midPara, UtilsTool.GetPropertyName(SubFieldExp));
                    if (left.Type.IsGenericType && left.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        left = Expression.Property(left, "Value");
                    }
                    if (left.Type == typeof(string))
                    {
                        left = Expression.Call(left, typeof(String).GetMethod("Trim", Type.EmptyTypes));
                    }
                    object vv = SubFieldExp.Compile().Invoke(li);
                    if (vv == null)
                    {
                        needBreak = true;
                        continue;
                    }
                    if (vv is string && vv.ToString() == "")
                    {
                        var requiredAttrs = li.GetType().GetProperty(UtilsTool.GetPropertyName(SubFieldExp)).GetCustomAttributes(typeof(RequiredAttribute), false);

                        if (requiredAttrs == null || requiredAttrs.Length == 0)
                        {
                            needBreak = true;
                            continue;
                        }
                        else
                        {
                            var requiredAtt = requiredAttrs[0] as RequiredAttribute;
                            if (requiredAtt.AllowEmptyStrings == true)
                            {
                                needBreak = true;
                                continue;
                            }
                        }
                    }

                    if (vv is string)
                    {
                        vv = vv.ToString().Trim();
                    }
                    ConstantExpression right = Expression.Constant(vv);
                    BinaryExpression equal = Expression.Equal(left, right);
                    innerExp.Add(equal);
                }
                if (needBreak)
                {
                    continue;
                }
                Expression exp = null;
                if (innerExp.Count == 1)
                {
                    exp = innerExp[0];
                }
                if (innerExp.Count > 1)
                {
                    exp = Expression.And(innerExp[0], innerExp[1]);
                    for (int i = 2; i < innerExp.Count; i++)
                    {
                        exp = Expression.And(exp, innerExp[i]);
                    }
                }
                if (exp != null)
                {
                    var any = Expression.Call(
                       typeof(Enumerable),
                       "Any",
                       new Type[] { typeof(V) },
                       Expression.Property(para, UtilsTool.GetPropertyName(MiddleExp)),
                       Expression.Lambda<Func<V, bool>>(exp, new ParameterExpression[] { midPara }));
                    allExp.Add(any);
                }
            }
            if (allExp.Count == 1)
            {
                rv = allExp[0];
            }
            if (allExp.Count > 1)
            {
                rv = Expression.Or(allExp[0], allExp[1]);
                for (int i = 2; i < allExp.Count; i++)
                {
                    rv = Expression.Or(rv, allExp[i]);
                }
            }
            return rv;
        }

        public List<PropertyInfo> GetProperties()
        {
            List<PropertyInfo> rv = new List<PropertyInfo>();
            foreach (var subField in SubFieldExps)
            {
                var pro = UtilsTool.GetPropertyInfo(subField);
                if (pro != null)
                {
                    rv.Add(pro);
                }
            }
            return rv;
        }
    }
}