using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace RecordCode
{
    public static class ObjectSetExtention
    {
        /// <summary>
        /// 存放資料表與資料欄對應
        /// </summary>
        private class TableMetadata
        {
            public EntitySet Table { get; set; }
            public Dictionary<string, EdmProperty> Properties { get; set; }
        }

        /// <summary>
        /// 批次更新
        /// </summary>
        public static int Update<TEntity>(this ObjectSet<TEntity> source, Expression<Func<TEntity>> setExpression, Expression<Func<TEntity, bool>> whereExpression) where TEntity : class
        {
            // 取得MSL資料
            var tableMetadata = GetTableMetadata<TEntity>(source);

            string setString = GetSetSql(setExpression, tableMetadata);
            string whereString = GetWhereSql(whereExpression, tableMetadata);

            string sql = string.Format("UPDATE {0}.{1} SET {2}{3}", tableMetadata.Table.MetadataProperties["Schema"].Value, tableMetadata.Table.Name, setString, whereString);
                        
            return source.Context.ExecuteStoreCommand(sql);            
        }

        /// <summary>
        /// 批次刪除
        /// </summary>
        public static int Delete<TEntity>(this ObjectSet<TEntity> source, Expression<Func<TEntity, bool>> whereExpression) where TEntity : class
        {
            // 取得MSL資料
            var tableMetadata = GetTableMetadata<TEntity>(source);

            string whereString = GetWhereSql(whereExpression, tableMetadata);

            string sql = string.Format("DELETE {0}.{1}{2}", tableMetadata.Table.MetadataProperties["Schema"].Value, tableMetadata.Table.Name, whereString);

            return source.Context.ExecuteStoreCommand(sql);
        }

        /// <summary>
        /// 取得Update中Set的Sql
        /// </summary>
        private static string GetSetSql<T>(Expression<Func<T>> setExpression, TableMetadata tableMetadata) where T : class
        {
            if (setExpression == null)
            {
                throw new ArgumentNullException("setExpression不可為NULL");
            }
            MemberInitExpression initExpression = setExpression.Body as MemberInitExpression;
            if (initExpression == null)
            {
                throw new ArgumentException("setExpression格式不正確");
            }

            List<MemberAssignment> memberAssignments = initExpression.Bindings.OfType<MemberAssignment>().ToList();

            var result = memberAssignments.Select(m => new { PropertyName = m.Member.Name, Value = GetExpressionValue(m.Expression) }).ToList();

            StringBuilder sb = new StringBuilder();

            var member = result[0];
            sb.AppendFormat("{0}={1}", tableMetadata.Properties[member.PropertyName].Name, member.Value);
            for (int i = 1; i < result.Count; i++)
            {
                member = result[i];
                sb.AppendFormat(",{0}={1}", tableMetadata.Properties[member.PropertyName].Name, member.Value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 取得Where的Sql
        /// </summary>
        private static string GetWhereSql<TEntity>(Expression<Func<TEntity, bool>> whereExpression, TableMetadata tableMetadata) where TEntity : class
        {
            if (whereExpression == null)
            {
                return null;
            }
            BinaryExpression binaryExpression = whereExpression.Body as BinaryExpression;
            if (binaryExpression == null)
            {
                throw new ArgumentException("whereExpression格式不正確");
            }

            string oper;

            string left = GetStringExpression(binaryExpression.Left as Expression, tableMetadata);
            string right = GetStringExpression(binaryExpression.Right as Expression, tableMetadata);

            //=NULL 換成 IS NULL !=NULL 換成 IS NOT NULL
            if (binaryExpression.NodeType == ExpressionType.Equal && right == "NULL")
            {
                oper = " IS ";
            }
            else if (binaryExpression.NodeType == ExpressionType.NotEqual && right == "NULL")
            {
                oper = " IS NOT ";
            }
            else
            {
                oper = GetOperator(binaryExpression.NodeType);
            }

            return string.Format(" WHERE ({0}{1}{2})", left, oper, right);
        }

        /// <summary>
        /// 取得MSL中Table的資訊
        /// </summary>
        private static TableMetadata GetTableMetadata<TEntity>(ObjectSet<TEntity> source) where TEntity : class
        {
            //執行EnsureMetadata後才會載入MSL
            typeof(ObjectContext).InvokeMember("EnsureMetadata", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, source.Context, null);

            //CSSpace就是MSL的DataSpace
            var mapContainer = source.Context.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace)[0];

            //因為微軟雖然有寫相關的Class或Method，但都是Internal的，所以只好用Reflection取資料。
            var mapSet = mapContainer.GetType().InvokeMember("GetSetMapping", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, mapContainer, new object[] { source.EntitySet.Name });
            var mapType = (mapSet.GetType().InvokeMember("TypeMappings", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, mapSet, null) as IList)[0];
            var map = (mapType.GetType().InvokeMember("MappingFragments", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, mapType, null) as IList)[0];

            var tableMetadata = new TableMetadata();

            tableMetadata.Table = map.GetType().InvokeMember("TableSet", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, map, null) as EntitySet;
            tableMetadata.Properties = new Dictionary<string, EdmProperty>();

            PropertyInfo pinfo = null, cpinfo = null;
            foreach (var item in (map.GetType().InvokeMember("Properties", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, map, null) as IEnumerable))
            {
                if (pinfo == null)
                {
                    cpinfo = item.GetType().GetProperty("ColumnProperty", BindingFlags.NonPublic | BindingFlags.Instance);
                    pinfo = item.GetType().GetProperty("EdmProperty", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                EdmProperty cprop = cpinfo.GetValue(item, null) as EdmProperty;
                EdmProperty prop = pinfo.GetValue(item, null) as EdmProperty;
                tableMetadata.Properties.Add(prop.Name, cprop);
            }
            return tableMetadata;
        }

        /// <summary>
        /// 以遞迴方式解析Where的Expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="tableMetadata"></param>
        /// <returns></returns>
        private static string GetStringExpression(Expression expression, TableMetadata tableMetadata)
        {
            if (expression is BinaryExpression)
            {
                //還是比較 And OR
                string oper, left, right;

                BinaryExpression binaryExpression = expression as BinaryExpression;
                left = GetStringExpression(binaryExpression.Left as Expression, tableMetadata);
                right = GetStringExpression(binaryExpression.Right as Expression, tableMetadata);

                //=NULL 換成 IS NULL !=NULL 換成 IS NOT NULL
                if (expression.NodeType == ExpressionType.Equal && right == "NULL")
                {
                    oper = " IS ";
                }
                else if (expression.NodeType == ExpressionType.NotEqual && right == "NULL")
                {
                    oper = " IS NOT ";

                }
                else
                {
                    oper = GetOperator(expression.NodeType);
                }

                return string.Format("({0}{1}{2})", left, oper, right);

            }
            else if (expression is MemberExpression)
            {
                MemberExpression memberExpression = expression as MemberExpression;
                if (memberExpression.Expression is ParameterExpression)
                {
                    //欄位
                    return tableMetadata.Properties[memberExpression.Member.Name].Name;
                }
            }

            return GetExpressionValue(expression);
        }

        /// <summary>
        /// 取得Expression值
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string GetExpressionValue(Expression expression)
        {
            if (expression is ConstantExpression)
            {
                //直接設值的Expression
                var ce = expression as ConstantExpression;
                return Format(ce.Value, ce.Type);
            }
            else if (expression is UnaryExpression)
            {
                //表示有一元 (Unary) 運算子的運算式
                UnaryExpression ue = expression as UnaryExpression;

                if (ue.Operand is MemberExpression)
                {
                    //取屬性值
                    MemberExpression me = ue.Operand as MemberExpression;
                    if (me.Type == typeof(DateTime))
                    {
                        //DateTime.Now 直接用SQL的語法
                        if (me.Member.Name == "Now")
                        {
                            return "GETDATE()";
                        }
                        else if (me.Member.Name == "UtcNow")
                        {
                            return "GETUTCDATE()";
                        }
                    }

                    return Format(Expression.Lambda(me).Compile().DynamicInvoke(), me.Type);
                }
                else
                {
                    return Format(Expression.Lambda(ue.Operand).Compile().DynamicInvoke(), ue.Operand.Type);
                }
            }

            return Format(Expression.Lambda(expression).Compile().DynamicInvoke(), expression.Type);
        }

        /// <summary>
        /// 取得操作子
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.OrElse:
                    return " OR ";
                default:
                    throw new ArgumentException("不支援的Where操作");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private static string Format(object value, Type elementType)
        {
            if (value == null)
            {
                return "NULL";
            }
            else
            {
                switch (elementType.Name)
                {
                    case "String":
                        return string.Format("'{0}'", value.ToString().Replace("'", "''"));
                    case "DateTime":
                        return string.Format("'{0:yyyy-MM-dd HH:mm:ss}'", value);
                    default:
                        return value.ToString();
                }
            }
        }
    }
}
