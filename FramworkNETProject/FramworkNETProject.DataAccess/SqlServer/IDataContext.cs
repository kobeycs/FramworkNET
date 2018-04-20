using System.Data.Entity;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using Models;
using Models.System;
using System.Data;

namespace DataAccess.SqlServer
{
    public interface IDataContext
    {
        IDbSet<T> GetSet<T>() where T : class;
        void SetStatus(object entity, System.Data.Entity.EntityState state);
        IDbSet<PageFunction> PageFunctions { get; }
        IDbSet<FunctionPrivilege> FunctionPrivileges { get; }
        IDbSet<ActionLog> ActionLogs { get; }
        void DeleteEntity<T>(IDbSet<T> dbSet, T entity) where T : BasePoco;
        void ChangeRelation<T>(T source, object target, Expression<Func<T, object>> navigation, System.Data.Entity.EntityState state) where T : class;
        void Detach(object entity);
        IEnumerable<T> RunSP<T>(string command, params object[] paras);

        IEnumerable<T> RunSP<T>(string command, int timeout, params object[] paras) where T : new();

        DataTable RunSP(string command, params object[] paras);

        DataTable RunSP(string command, int timeout, params object[] paras);

        DataSet RunSPDataSet(string command, params object[] paras);
        
        void DisableChangeDetection();
        void EnableChangeDetection();

        IDataContext RecreateDC();
        bool Commited { get; }
        void Commit();
        void SaveCommit();
    }
}
