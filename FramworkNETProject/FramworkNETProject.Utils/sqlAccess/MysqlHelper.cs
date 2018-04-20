using System;
using System.Data;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Collections;

namespace Utils
{
    /// <summary>
    /// The MysqlHelper class is intended to encapsulate high performance, scalable best practices for 
    /// common uses of SqlClient
    /// </summary>
    public sealed class MysqlHelper
    {
        #region private utility methods & constructors

        // Since this class provides only static methods, make the default constructor private to prevent 
        // instances from being created with "new MysqlHelper()"
        private MysqlHelper() { }

        /// <summary>
        /// This method is used to attach array of SqlParameters to a MySqlCommand.
        /// 
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.  
        /// 
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
        private static void AttachParameters(MySqlCommand command, MySqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (MySqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // Check for derived output value with no value assigned
                        if ((p.Direction == ParameterDirection.InputOutput ||
                            p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// This method assigns dataRow column values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        private static void AssignParameterValues(MySqlParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                // Do nothing if we get no data
                return;
            }

            int i = 0;
            // Set the parameters values
            foreach (MySqlParameter commandParameter in commandParameters)
            {
                // Check the parameter name
                if (commandParameter.ParameterName == null ||
                    commandParameter.ParameterName.Length <= 1)
                    throw new Exception(
                        string.Format(
                            "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                            i, commandParameter.ParameterName));
                if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                i++;
            }
        }

        /// <summary>
        /// This method assigns an array of values to an array of SqlParameters
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private static void AssignParameterValues(MySqlParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                // Do nothing if we get no data
                return;
            }

            // We must have the same number of values as we pave parameters to put them in
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            // Iterate through the SqlParameters, assigning the values from the corresponding position in the 
            // value array
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                // If the current array value derives from IDbDataParameter, then assign its Value property
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                    if (paramInstance.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = paramInstance.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        /// to the provided command
        /// </summary>
        /// <param name="command">The MySqlCommand to be prepared</param>
        /// <param name="connection">A valid MySqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid MySqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
        private static void PrepareCommand(MySqlCommand command, MySqlConnection connection, MySqlTransaction transaction, CommandType commandType, string commandText, MySqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // Associate the connection with the command
            command.Connection = connection;

            // Set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            // If we were provided a transaction, assign it
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }

            // Set the command type
            command.CommandType = commandType;

            // Attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        #endregion private utility methods & constructors

        #region ExecuteNonQuery

        /// <summary>
        /// Execute a MySqlCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(connectionString, commandType, commandText, (MySqlParameter[])null);
        }
        public static int ExecuteNonQuery(string commandText, params MySqlParameter[] commandParameters)
        {
            return ExecuteNonQuery(WebConfig.DefaultConnectionString, CommandType.Text, commandText, commandParameters);
        }
        /// <summary>
        /// Execute a MySqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a MySqlConnection, and dispose of it after we are done
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns no resultset and takes no parameters) against the provided MySqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(MySqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(connection, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns no resultset) against the specified MySqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(MySqlConnection connection, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            MySqlParameter[] param = SetParameters(commandParameters);
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            MySqlCommand cmd = new MySqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (MySqlTransaction)null, commandType, commandText, param, out mustCloseConnection);

            // Finally, execute the command
            int retval = cmd.ExecuteNonQuery();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns no resultset) against the specified MySqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(MySqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns no resultset and takes no parameters) against the provided MySqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(MySqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteNonQuery(transaction, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns no resultset) against the specified MySqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(MySqlTransaction transaction, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            MySqlParameter[] param = SetParameters(commandParameters);
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            MySqlCommand cmd = new MySqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            int retval = cmd.ExecuteNonQuery();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns no resultset) against the specified 
        /// MySqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(MySqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteNonQuery
        
        #region ExecuteDataset
        public static DataSet ExecuteDataset(string sqltext)
        {
            DataSet myDataSet = new DataSet();

            MySqlConnection myConnection = new MySqlConnection();

            try
            {
                myConnection = new MySqlConnection(WebConfig.DefaultConnectionString);
                myConnection.Open();
                MySqlDataAdapter myAdapter = new MySqlDataAdapter(sqltext, myConnection);
                myAdapter.Fill(myDataSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                myConnection.Close();
            }
            return myDataSet;
        }
        public static DataSet ExecuteDataset(string connectionstr, string sqltext)
        {
            DataSet myDataSet = new DataSet();

            MySqlConnection myConnection = new MySqlConnection();

            try
            {
                myConnection = new MySqlConnection(connectionstr);
                myConnection.Open();
                MySqlDataAdapter myAdapter = new MySqlDataAdapter(sqltext, myConnection);
                myAdapter.Fill(myDataSet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                myConnection.Close();
            }
            return myDataSet;
        }

        public static DataSet ExecuteDataset(string sqltext, MySqlParameter[] commandParameters)
        {
            string connectionString = WebConfig.DefaultConnectionString;
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a MySqlConnection, and dispose of it after we are done
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteDataset(connection, CommandType.Text, sqltext, commandParameters);
            }
        }
        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(connectionString, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            // Create & open a MySqlConnection, and dispose of it after we are done
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the provided MySqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(MySqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(connection, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(MySqlConnection connection, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            MySqlCommand cmd = new MySqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (MySqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                    connection.Close();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(MySqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the provided MySqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(MySqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteDataset(transaction, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset) against the specified MySqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(MySqlTransaction transaction, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            MySqlCommand cmd = new MySqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                // Return the dataset
                return ds;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified 
        /// MySqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(MySqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteDataset

        #region ExecuteReader

        /// <summary>
        /// This enum is used to indicate whether the connection was provided by the caller, or created by MysqlHelper, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        private enum SqlConnectionOwnership
        {
            /// <summary>Connection is owned and managed by MysqlHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }

        /// <summary>
        /// Create and prepare a MySqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        /// 
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid MySqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by MysqlHelper</param>
        /// <returns>MySqlDataReader containing the results of the command</returns>
        private static MySqlDataReader ExecuteReader(MySqlConnection connection, MySqlTransaction transaction, CommandType commandType, string commandText, MySqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;
            // Create a command and prepare it for execution
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                // Create a reader
                MySqlDataReader dataReader;

                // Call ExecuteReader with the appropriate CommandBehavior
                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }

                // Detach the SqlParameters from the command object, so they can be used again.
                // HACK: There is a problem here, the output parameter values are fletched 
                // when the reader is closed, so if the parameters are detached from the command
                // then the SqlReader can磘 set its values. 
                // When this happen, the parameters can磘 be used again in other command.
                bool canClear = true;
                foreach (MySqlParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                        canClear = false;
                }

                if (canClear)
                {
                    cmd.Parameters.Clear();
                }

                if (mustCloseConnection)
                    connection.Close();

                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                    connection.Close();
                throw;
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteReader(connectionString, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();

                // Call the private overload that takes an internally owned connection in place of the connection string
                return ExecuteReader(connection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
            }
            catch
            {
                // If we fail to return the SqlDatReader, we need to close the connection ourselves
                if (connection != null) connection.Close();
                throw;
            }

        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the provided MySqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(MySqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteReader(connection, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(MySqlConnection connection, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            // Pass through the call to the private overload using a null transaction value and an externally owned connection
            return ExecuteReader(connection, (MySqlTransaction)null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(MySqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the provided MySqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(MySqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteReader(transaction, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a resultset) against the specified MySqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///   MySqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(MySqlTransaction transaction, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Pass through to private overload, indicating that the connection is owned by the caller
            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified
        /// MySqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  MySqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReader(MySqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteReader

        #region ExecuteScalar

        /// <summary>
        /// Execute a MySqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(connectionString, commandType, commandText, (MySqlParameter[])null);
        }
        public static object ExecuteScalar(string connectionstr, string commandText, MySqlParameter[] commandParameters)
        {
            using (MySqlConnection cn = new MySqlConnection(connectionstr))
            {
                cn.Open();

                return ExecuteScalar(cn, CommandType.Text, commandText, commandParameters);
            }
        }
        /// <summary>
        /// Execute a MySqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            MySqlParameter[] param = SetParameters(commandParameters);
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            // Create & open a MySqlConnection, and dispose of it after we are done
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteScalar(connection, commandType, commandText, param);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided MySqlConnection. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(MySqlConnection connection, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(connection, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a 1x1 resultset) against the specified MySqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(MySqlConnection connection, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            MySqlCommand cmd = new MySqlCommand();

            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (MySqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a 1x1 resultset) against the specified MySqlConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(MySqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided MySqlTransaction. 
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(MySqlTransaction transaction, CommandType commandType, string commandText)
        {
            // Pass through the call providing null for the set of SqlParameters
            return ExecuteScalar(transaction, commandType, commandText, (MySqlParameter[])null);
        }

        /// <summary>
        /// Execute a MySqlCommand (that returns a 1x1 resultset) against the specified MySqlTransaction
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new MySqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(MySqlTransaction transaction, CommandType commandType, string commandText, params MySqlParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // Create a command and prepare it for execution
            MySqlCommand cmd = new MySqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // Execute the command & return the results
            object retval = cmd.ExecuteScalar();

            // Detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a 1x1 resultset) against the specified
        /// MySqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// 
        /// e.g.:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid MySqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalar(MySqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If we receive parameter values, we need to figure out where they go
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // PPull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues);

                // Call the overload that takes an array of SqlParameters
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // Otherwise we can just call the SP without params
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteScalar

        #region FillDataset
        ///// <summary>
        ///// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        ///// the connection string. 
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///// </remarks>
        ///// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        ///// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">The stored procedure name or T-SQL command</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)</param>
        //public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        //{
        //    if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        //    if (dataSet == null) throw new ArgumentNullException("dataSet");

        //    // Create & open a MySqlConnection, and dispose of it after we are done
        //    using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        connection.Open();

        //        // Call the overload that takes a connection in place of the connection string
        //        FillDataset(connection, commandType, commandText, dataSet, tableNames);
        //    }
        //}

        ///// <summary>
        ///// Execute a MySqlCommand (that returns a resultset) against the database specified in the connection string 
        ///// using the provided parameters.
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new MySqlParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        ///// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">The stored procedure name or T-SQL command</param>
        ///// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>
        //public static void FillDataset(string connectionString, CommandType commandType,
        //    string commandText, DataSet dataSet, string[] tableNames,
        //    params MySqlParameter[] commandParameters)
        //{
        //    if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        //    if (dataSet == null) throw new ArgumentNullException("dataSet");
        //    // Create & open a MySqlConnection, and dispose of it after we are done
        //    using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        connection.Open();

        //        // Call the overload that takes a connection in place of the connection string
        //        FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
        //    }
        //}

        ///// <summary>
        ///// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the database specified in 
        ///// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///// </summary>
        ///// <remarks>
        ///// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///// 
        ///// e.g.:  
        /////  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        ///// </remarks>
        ///// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        ///// <param name="spName">The name of the stored procedure</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>    
        ///// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        //public static void FillDataset(string connectionString, string spName,
        //    DataSet dataSet, string[] tableNames,
        //    params object[] parameterValues)
        //{
        //    if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
        //    if (dataSet == null) throw new ArgumentNullException("dataSet");
        //    // Create & open a MySqlConnection, and dispose of it after we are done
        //    using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        connection.Open();

        //        // Call the overload that takes a connection in place of the connection string
        //        FillDataset(connection, spName, dataSet, tableNames, parameterValues);
        //    }
        //}

        ///// <summary>
        ///// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the provided MySqlConnection. 
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///// </remarks>
        ///// <param name="connection">A valid MySqlConnection</param>
        ///// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">The stored procedure name or T-SQL command</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>    
        //public static void FillDataset(MySqlConnection connection, CommandType commandType,
        //    string commandText, DataSet dataSet, string[] tableNames)
        //{
        //    FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        //}

        ///// <summary>
        ///// Execute a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        ///// using the provided parameters.
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new MySqlParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connection">A valid MySqlConnection</param>
        ///// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">The stored procedure name or T-SQL command</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>
        ///// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        //public static void FillDataset(MySqlConnection connection, CommandType commandType,
        //    string commandText, DataSet dataSet, string[] tableNames,
        //    params MySqlParameter[] commandParameters)
        //{
        //    FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        //}

        ///// <summary>
        ///// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        ///// using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///// </summary>
        ///// <remarks>
        ///// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///// 
        ///// e.g.:  
        /////  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        ///// </remarks>
        ///// <param name="connection">A valid MySqlConnection</param>
        ///// <param name="spName">The name of the stored procedure</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>
        ///// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        //public static void FillDataset(MySqlConnection connection, string spName,
        //    DataSet dataSet, string[] tableNames,
        //    params object[] parameterValues)
        //{
        //    if (connection == null) throw new ArgumentNullException("connection");
        //    if (dataSet == null) throw new ArgumentNullException("dataSet");
        //    if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        //    // If we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

        //        // Assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        // Call the overload that takes an array of SqlParameters
        //        FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
        //    }
        //    else
        //    {
        //        // Otherwise we can just call the SP without params
        //        FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
        //    }
        //}

        ///// <summary>
        ///// Execute a MySqlCommand (that returns a resultset and takes no parameters) against the provided MySqlTransaction. 
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///// </remarks>
        ///// <param name="transaction">A valid MySqlTransaction</param>
        ///// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">The stored procedure name or T-SQL command</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>
        //public static void FillDataset(MySqlTransaction transaction, CommandType commandType,
        //    string commandText,
        //    DataSet dataSet, string[] tableNames)
        //{
        //    FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        //}

        ///// <summary>
        ///// Execute a MySqlCommand (that returns a resultset) against the specified MySqlTransaction
        ///// using the provided parameters.
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new MySqlParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="transaction">A valid MySqlTransaction</param>
        ///// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">The stored procedure name or T-SQL command</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>
        ///// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        //public static void FillDataset(MySqlTransaction transaction, CommandType commandType,
        //    string commandText, DataSet dataSet, string[] tableNames,
        //    params MySqlParameter[] commandParameters)
        //{
        //    FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        //}

        ///// <summary>
        ///// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified 
        ///// MySqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///// </summary>
        ///// <remarks>
        ///// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///// 
        ///// e.g.:  
        /////  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        ///// </remarks>
        ///// <param name="transaction">A valid MySqlTransaction</param>
        ///// <param name="spName">The name of the stored procedure</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>
        ///// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        //public static void FillDataset(MySqlTransaction transaction, string spName,
        //    DataSet dataSet, string[] tableNames,
        //    params object[] parameterValues)
        //{
        //    if (transaction == null) throw new ArgumentNullException("transaction");
        //    if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
        //    if (dataSet == null) throw new ArgumentNullException("dataSet");
        //    if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

        //    // If we receive parameter values, we need to figure out where they go
        //    if ((parameterValues != null) && (parameterValues.Length > 0))
        //    {
        //        // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
        //        MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

        //        // Assign the provided values to these parameters based on parameter order
        //        AssignParameterValues(commandParameters, parameterValues);

        //        // Call the overload that takes an array of SqlParameters
        //        FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
        //    }
        //    else
        //    {
        //        // Otherwise we can just call the SP without params
        //        FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
        //    }
        //}

        ///// <summary>
        ///// Private helper method that execute a MySqlCommand (that returns a resultset) against the specified MySqlTransaction and MySqlConnection
        ///// using the provided parameters.
        ///// </summary>
        ///// <remarks>
        ///// e.g.:  
        /////  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new MySqlParameter("@prodid", 24));
        ///// </remarks>
        ///// <param name="connection">A valid MySqlConnection</param>
        ///// <param name="transaction">A valid MySqlTransaction</param>
        ///// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        ///// <param name="commandText">The stored procedure name or T-SQL command</param>
        ///// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        ///// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///// by a user defined name (probably the actual table name)
        ///// </param>
        ///// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        //private static void FillDataset(MySqlConnection connection, MySqlTransaction transaction, CommandType commandType,
        //    string commandText, DataSet dataSet, string[] tableNames,
        //    params MySqlParameter[] commandParameters)
        //{
        //    if (connection == null) throw new ArgumentNullException("connection");
        //    if (dataSet == null) throw new ArgumentNullException("dataSet");

        //    // Create a command and prepare it for execution
        //    MySqlCommand command = new MySqlCommand();
        //    bool mustCloseConnection = false;
        //    PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

        //    // Create the DataAdapter & DataSet
        //    using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command))
        //    {

        //        // Add the table mappings specified by the user
        //        if (tableNames != null && tableNames.Length > 0)
        //        {
        //            string tableName = "Table";
        //            for (int index = 0; index < tableNames.Length; index++)
        //            {
        //                if (tableNames[index] == null || tableNames[index].Length == 0) throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
        //                dataAdapter.TableMappings.Add(tableName, tableNames[index]);
        //                tableName += (index + 1).ToString();
        //            }
        //        }

        //        // Fill the DataSet using default values for DataTable names, etc
        //        dataAdapter.Fill(dataSet);

        //        // Detach the SqlParameters from the command object, so they can be used again
        //        command.Parameters.Clear();
        //    }

        //    if (mustCloseConnection)
        //        connection.Close();
        //}
        #endregion

        #region UpdateDataset
        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param>
        /// <param name="dataSet">The DataSet used to update the data source</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public static void UpdateDataset(MySqlCommand insertCommand, MySqlCommand deleteCommand, MySqlCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null) throw new ArgumentNullException("insertCommand");
            if (deleteCommand == null) throw new ArgumentNullException("deleteCommand");
            if (updateCommand == null) throw new ArgumentNullException("updateCommand");
            if (tableName == null || tableName.Length == 0) throw new ArgumentNullException("tableName");

            // Create a MySqlDataAdapter, and dispose of it after we are done
            using (MySqlDataAdapter dataAdapter = new MySqlDataAdapter())
            {
                // Set the data adapter commands
                dataAdapter.UpdateCommand = updateCommand;
                dataAdapter.InsertCommand = insertCommand;
                dataAdapter.DeleteCommand = deleteCommand;

                // Update the dataset changes in the data source
                dataAdapter.Update(dataSet, tableName);

                // Commit all the changes made to the DataSet
                dataSet.AcceptChanges();
            }
        }
        #endregion

        #region CreateCommand
        /// <summary>
        /// Simplify the creation of a Sql command object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  MySqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid MySqlCommand object</returns>
        public static MySqlCommand CreateCommand(MySqlConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // Create a MySqlCommand
            MySqlCommand cmd = new MySqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            // If we receive parameter values, we need to figure out where they go
            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Assign the provided source columns to these parameters based on parameter order
                for (int index = 0; index < sourceColumns.Length; index++)
                    commandParameters[index].SourceColumn = sourceColumns[index];

                // Attach the discovered parameters to the MySqlCommand object
                AttachParameters(cmd, commandParameters);
            }

            return cmd;
        }
        #endregion

        #region ExecuteNonQueryTypedParams
        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns no resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQueryTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns no resultset) against the specified MySqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.  
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQueryTypedParams(MySqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns no resultset) against the specified
        /// MySqlTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid MySqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQueryTypedParams(MySqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // Sf the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion

        #region ExecuteDatasetTypedParams
        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDatasetTypedParams(string connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            //If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        /// using the dataRow column values as the store procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDatasetTypedParams(MySqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified MySqlTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid MySqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDatasetTypedParams(MySqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion

        #region ExecuteReaderTypedParams
        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReaderTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }


        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified MySqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReaderTypedParams(MySqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a resultset) against the specified MySqlTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid MySqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A MySqlDataReader containing the resultset generated by the command</returns>
        public static MySqlDataReader ExecuteReaderTypedParams(MySqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion

        #region ExecuteScalarTypedParams
        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalarTypedParams(String connectionString, String spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a 1x1 resultset) against the specified MySqlConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalarTypedParams(MySqlConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a MySqlCommand (that returns a 1x1 resultset) against the specified MySqlTransaction
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid MySqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public static object ExecuteScalarTypedParams(MySqlTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // If the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // Pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                MySqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                // Set the parameters values
                AssignParameterValues(commandParameters, dataRow);

                return MysqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return MysqlHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion

        #region SetParameters
        /// <summary>
        /// 将时间类型的参数换成null，操作数据库的时候时间溢出
        /// </summary>
        /// <param name="commandParameters"></param>
        /// <returns></returns>
        public static MySqlParameter[] SetParameters(MySqlParameter[] commandParameters)
        {
            MySqlParameter[] paramResult = commandParameters;
            string str = string.Empty;
            if (paramResult == null)
                return paramResult;
            for (int i = 0; i < paramResult.Length; i++)
            {

                if (paramResult[i].DbType.ToString() == "DateTime" && paramResult[i].Value != null && (DateTime)paramResult[i].Value == Convert.ToDateTime(null))
                    paramResult[i].Value = null;
            }
            return paramResult;
        }
        #endregion

        /// <summary>
        /// 带分页的绑定
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="OrderByValue">排序字段</param>
        /// <param name="value">显示字段</param>
        /// <param name="PageSize">每页显示多少</param>
        /// <param name="PageIndex">第几页</param>
        /// <param name="RecordCount">总条数</param>
        /// <param name="orderType">desc asc 排序</param>
        /// <param name="StrWhere">查询条件</param>
        /// <returns></returns>
        public static DataSet BandPager(string TableName, string OrderByValue, string value, int PageSize, int PageIndex,  bool orderType, string StrWhere)
        {
            string sqlStr = SqlSplit.GetPageSql(TableName, OrderByValue, value, PageSize, PageIndex, orderType, " 1=1 " + StrWhere);
            DataSet ds = MysqlHelper.ExecuteDataset(sqlStr);
            return ds;
        }

        /// <summary>
        /// 带分页/带参数的绑定
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="OrderByValue">排序字段</param>
        /// <param name="value">显示字段</param>
        /// <param name="PageSize">每页显示多少</param>
        /// <param name="PageIndex">第几页</param>
        /// <param name="RecordCount">总条数</param>
        /// <param name="orderType">desc asc 排序</param>
        /// <param name="StrWhere">查询条件</param>
        /// <returns></returns>
        public static DataSet BandPager(string TableName, string OrderByValue, string value, int PageSize, int PageIndex, bool orderType, string StrWhere, params MySqlParameter[] commandParameters)
        {
            string sqlStr = SqlSplit.GetPageSql(TableName, OrderByValue, value, PageSize, PageIndex, orderType, " 1=1 " + StrWhere);
            DataSet ds = MysqlHelper.ExecuteDataset(sqlStr, commandParameters);
            return ds;
        }
    }

    /// <summary>
    /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
    /// ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class SqlHelperParameterCache
    {
        #region private methods, variables, and constructors

        //Since this class provides only static methods, make the default constructor private to prevent 
        //instances from being created with "new SqlHelperParameterCache()"
        private SqlHelperParameterCache() { }

        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Resolve at run time the appropriate set of SqlParameters for a stored procedure
        /// </summary>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        private static MySqlParameter[] DiscoverSpParameterSet(MySqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            MySqlCommand cmd = new MySqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            MySqlCommandBuilder.DeriveParameters(cmd);
            connection.Close();

            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }

            MySqlParameter[] discoveredParameters = new MySqlParameter[cmd.Parameters.Count];

            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // Init the parameters with a DBNull value
            foreach (MySqlParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

        /// <summary>
        /// Deep copy of cached MySqlParameter array
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        private static MySqlParameter[] CloneParameters(MySqlParameter[] originalParameters)
        {
            MySqlParameter[] clonedParameters = new MySqlParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (MySqlParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion private methods, variables, and constructors

        #region caching functions

        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params MySqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An array of SqlParamters</returns>
        public static MySqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = connectionString + ":" + commandText;

            MySqlParameter[] cachedParameters = paramCache[hashKey] as MySqlParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion caching functions

        #region Parameter Discovery Functions

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        public static MySqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a MySqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        public static MySqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        internal static MySqlParameter[] GetSpParameterSet(MySqlConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        internal static MySqlParameter[] GetSpParameterSet(MySqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            using (MySqlConnection clonedConnection = (MySqlConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid MySqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        private static MySqlParameter[] GetSpParameterSetInternal(MySqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            MySqlParameter[] cachedParameters;

            cachedParameters = paramCache[hashKey] as MySqlParameter[];
            if (cachedParameters == null)
            {
                MySqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }

        #endregion Parameter Discovery Functions

    }
}
