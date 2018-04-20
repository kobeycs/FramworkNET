using Models.System;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace DataAccess.SqlServer
{
    public class FillDataHelper
    {
        public static void FillData(IDataContext context)
        {
            try
            {

            }
            catch (Exception)
            {
            }
        }

        public static void FillUITestData(IDataContext context)
        {
        }
    }

    public class StandardInitializer : CreateDatabaseIfNotExists<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            FillDataHelper.FillData(context);
        }
    }

    public class ReCreateInitializer : DropCreateDatabaseAlways<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            FillDataHelper.FillData(context);
        }
    }

    public class ReCreateWhenModifiedInitializer : DropCreateDatabaseIfModelChanges<DataContext>
    {
        protected override void Seed(DataContext context)
        {
            FillDataHelper.FillData(context);
        }
    }
}