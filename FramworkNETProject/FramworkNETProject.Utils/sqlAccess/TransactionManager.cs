using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace Utils
{
    public class TransactionManager : IDisposable
    {
        private MySqlConnection currentSqlConnection = null;

        private MySqlTransaction trans = null;

        public TransactionManager()
        {
            currentSqlConnection = new MySqlConnection(WebConfig.DefaultConnectionString);
            try
            {
                if (currentSqlConnection.State == System.Data.ConnectionState.Closed)
                {
                    currentSqlConnection.Open();
                }
            }
            catch
            {
                currentSqlConnection.Open();
            }

            if (currentSqlConnection.State == System.Data.ConnectionState.Open)
            {
                trans = currentSqlConnection.BeginTransaction();
            }
        }

        public MySqlTransaction Trans
        {
            get
            {
                return trans;
            }
        }

        public void Dispose()
        {
            if (currentSqlConnection.State == System.Data.ConnectionState.Open)
            {
                currentSqlConnection.Close();
            }
            currentSqlConnection.Dispose();
        }
    }
}
