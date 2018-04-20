using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Entity.Validation;
using Models.System;
using Models;
using Utils;

namespace DataAccess.SqlServer
{
    public class DataContext : DbContext, IDataContext
    {
        public enum InitializerTypes { 标准, 永远重新生成, 改变时重新生成 }
        public bool Commited { get; set; }

        public IDbSet<PageFunction> PageFunctions { get; private set; }
        public IDbSet<FunctionPrivilege> FunctionPrivileges { get; private set; }
        public IDbSet<ActionLog> ActionLogs { get; private set; }

        public DataContext()
            : base(DataContext.GetCS())
        {
            InitDbSets();
        }

        public DataContext(string cs)
            : base(cs)
        {
            InitDbSets();
        }

        private void InitDbSets()
        {
            PageFunctions = this.Set<PageFunction>();
            FunctionPrivileges = this.Set<FunctionPrivilege>();
            ActionLogs = this.Set<ActionLog>();

        }

        public IDbSet<T> GetSet<T>() where T : class
        {
            return this.Set<T>();
        }

        public void SetStatus(object entity, System.Data.Entity.EntityState state)
        {
            this.Entry(entity).State = state;
        }

        public void DeleteEntity<T>(IDbSet<T> dbSet, T entity) where T : BasePoco
        {
            SetStatus(entity, System.Data.Entity.EntityState.Deleted);
        }

        public void ChangeRelation<T>(T source, object target, Expression<Func<T, object>> navigation, System.Data.Entity.EntityState state) where T : class
        {
            ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.ChangeRelationshipState<T>(source, target, navigation, state);
        }

        public IEnumerable<T> RunSP<T>(string command, params object[] paras)
        {
            return this.Database.SqlQuery<T>(command, paras);

            //return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<T>(command, paras);
        }

        public IEnumerable<T> RunSP<T>(string command, int timeout, params object[] paras) where T : new()
        {
            if (timeout < 30)
            {
                timeout = 300;
            }
            System.Data.DataTable DataTB = RunSP(command, timeout, paras);
            return Utils.ModelConvertHelper<T>.ConvertToModel(DataTB);
            //return this.Database.SqlQuery<T>(command, paras);

            //return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<T>(command, paras);
        }

        public System.Data.DataTable RunSP(string command, params object[] paras)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();

            SqlConnection con = this.Database.Connection as SqlConnection;
            using (SqlCommand cmd = new SqlCommand(command, con))
            {
                adapter.SelectCommand = cmd;
                cmd.CommandType = CommandType.StoredProcedure;
                if (paras != null)
                {
                    foreach (var param in paras)
                        cmd.Parameters.Add(param);
                }

                DataTable table = new DataTable();
                adapter.Fill(table);
                adapter.SelectCommand.Parameters.Clear();

                return table;
            }
        }
        public DataTable RunSP(string command, int timeout, params object[] paras)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();

            SqlConnection con = this.Database.Connection as SqlConnection;
            using (SqlCommand cmd = new SqlCommand(command, con))
            {
                adapter.SelectCommand = cmd;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeout;
                if (paras != null)
                {
                    foreach (var param in paras)
                        cmd.Parameters.Add(param);
                }

                DataTable table = new DataTable();
                adapter.Fill(table);
                adapter.SelectCommand.Parameters.Clear();

                return table;
            }
        }

        public DataTable RunView(string command, int timeout, params object[] paras)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();

            SqlConnection con = this.Database.Connection as SqlConnection;
            using (SqlCommand cmd = new SqlCommand(command, con))
            {
                adapter.SelectCommand = cmd;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = timeout;
                if (paras != null)
                {
                    foreach (var param in paras)
                        cmd.Parameters.Add(param);
                }

                System.Data.DataTable table = new System.Data.DataTable();
                adapter.Fill(table);
                adapter.SelectCommand.Parameters.Clear();

                return table;
            }
        }

        public System.Data.DataSet RunSPDataSet(string command, params object[] paras)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();

            SqlConnection con = this.Database.Connection as SqlConnection;
            using (SqlCommand cmd = new SqlCommand(command, con))
            {
                adapter.SelectCommand = cmd;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                if (paras != null)
                {
                    foreach (var param in paras)
                        cmd.Parameters.Add(param);
                }

                System.Data.DataSet ds = new System.Data.DataSet();
                adapter.Fill(ds);
                adapter.SelectCommand.Parameters.Clear();

                return ds;
            }
        }

        public void Detach(object entity)
        {
            ((IObjectContextAdapter)this).ObjectContext.Detach(entity);
        }

        public void DisableChangeDetection()
        {
            this.Configuration.AutoDetectChangesEnabled = false;
        }

        public void EnableChangeDetection()
        {
            this.Configuration.AutoDetectChangesEnabled = true;
        }

        public IDataContext RecreateDC()
        {
            this.Dispose();
            return new DataContext();
        }

        public static string GetCS()
        {
            string cs;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                cs = System.Configuration.ConfigurationManager.ConnectionStrings["Development"].ConnectionString;
            }
            else
            {
                cs = System.Configuration.ConfigurationManager.ConnectionStrings["Release"].ConnectionString;
            }
            return cs;
        }

        public static void SetInitializer(InitializerTypes InitType)
        {
            switch (InitType)
            {
                case InitializerTypes.标准:
                    Database.SetInitializer(new StandardInitializer());
                    break;
                case InitializerTypes.永远重新生成:
                    Database.SetInitializer(new ReCreateInitializer());
                    break;
                case InitializerTypes.改变时重新生成:
                    Database.SetInitializer(new ReCreateWhenModifiedInitializer());
                    break;
                default:
                    break;
            }
        }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Organization_Department>().HasRequired(x => x.Payroll_PayrollSystem).WithMany().WillCascadeOnDelete(false);
            //modelBuilder.Entity<Organization_Department>().HasRequired(x => x.Factory).WithMany().WillCascadeOnDelete(false);
            //modelBuilder.Entity<HR_Employee>().HasRequired(x => x.Department).WithMany().WillCascadeOnDelete(false);
            //modelBuilder.Entity<HR_TaxesArea>().HasRequired(x => x.Factory).WithMany().WillCascadeOnDelete(false);
            //modelBuilder.Entity<Payroll_PayrollSystem>().HasRequired(x => x.Organization_Factory).WithMany().HasForeignKey(x=>x.Organization_FactoryID).WillCascadeOnDelete(false);
            //modelBuilder.Entity<PageFunction>().HasMany(x => x.MLContents).WithRequired(y=>y.MLParent).HasForeignKey(x => x.MLParentID);
            //modelBuilder.Entity<ActionLog>().HasMany(x => x.MLContents).WithRequired().HasForeignKey(x => x.MLParentID);
            //modelBuilder.Entity<MLContent>().ToTable("MLContent");
            //modelBuilder.Entity<GroupNameMLContent>().ToTable("GroupNameMLContent");
            //modelBuilder.Entity<RoleNameMLContent>().ToTable("RoleNameMLContent");
            //modelBuilder.Entity<AttendanceMonthResult>().Property(x => x.ActualAbsence).HasPrecision(18, 4);
            //modelBuilder.Entity<AttendanceMonthResult>().Property(x => x.ActualAttendance).HasPrecision(18, 4);
            base.OnModelCreating(modelBuilder);
        }


        public void Commit()
        {
            ////报错查看
            try
            {
                this.SaveChanges();
            }
            catch (Exception ex)
            {
                int i = 0;
            }
            this.Commited = true;
        }

        /// <summary>
        /// 抛出错误，不能隐藏，免得外面捕捉不到
        /// </summary>
        public void SaveCommit()
        {

            this.SaveChanges();
            this.Commited = true;
        }

        public static void Init()
        {
            Database.DefaultConnectionFactory = new SqlConnectionFactory();
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DataContext.SetInitializer(DataContext.InitializerTypes.标准);
            }
            else
            {
                DataContext.SetInitializer(DataContext.InitializerTypes.标准);
            }
        }
    }

    public static class DCExtentions
    {
        public static IQueryable<T> Include<T>(this IQueryable<T> dbset, Expression<Func<T, object>> exp) where T : BasePoco
        {
            string name = GetPropertyName(exp);
            return dbset.Include(name) as IQueryable<T>;
        }

        public static IQueryable<T> SInclude<T>(this IQueryable<T> dbset, string PropertyName) where T : BasePoco
        {
            return dbset.Include(PropertyName) as IQueryable<T>;
        }

        public static IQueryable<T> NoTracking<T>(this IQueryable<T> dbset) where T : BasePoco
        {

            return dbset.AsNoTracking();
        }

        public static string GetPropertyName(LambdaExpression expression, bool getAll = true)
        {
            MemberExpression me = null;
            LambdaExpression le = expression as LambdaExpression;
            if (le.Body is MemberExpression)
            {
                me = le.Body as MemberExpression;
            }
            if (le.Body is UnaryExpression)
            {
                me = (le.Body as UnaryExpression).Operand as MemberExpression;
            }
            string rv = "";
            if (me != null)
            {
                rv = me.Member.Name;
            }
            while (me != null && getAll && me.NodeType == ExpressionType.MemberAccess)
            {
                Expression exp = me.Expression;
                if (exp is MemberExpression)
                {
                    rv = (exp as MemberExpression).Member.Name + "." + rv;
                    me = exp as MemberExpression;
                }
                else
                {
                    break;
                }
            }
            return rv;
        }

    }
}