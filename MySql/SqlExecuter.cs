using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BeeEeeLibs.Mysql
{
    /// <summary>
    /// Executes the sql with the given parameters
    /// </summary>
    public class SqlExecuter
    {
        private bool UseTransaction;
        private string Sql;
        private IDbDataParameter[] Parameters;
        private int CommandTimeout;
        private string ConnectionString;
        private MySqlConnection Connection;
        private bool DisoposeConnection;

        /// <summary>
        /// Create and start building the context of the SQL to execute
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="sql">The SQL</param>
        /// <returns>An SqlExecuter that holds the context</returns>
        public static SqlExecuter Start(MySqlConnection connection, string sql)
        {
            return new SqlExecuter(connection, sql);
        }

        /// <summary>
        /// Create and start building the context of the SQL to execute.
        /// </summary>
        /// <param name="connectionString">A connection string so that a connection can be created</param>
        /// <param name="sql">The SQL</param>
        /// <param name="commandTimeout">The ammount of seconds before a command times out.</param>
        /// <returns>The SqlExecuter that contains the context</returns>
        public static SqlExecuter Start(string connectionString, string sql, int commandTimeout = 30)
        {
            return new SqlExecuter(connectionString, sql, commandTimeout);
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="connection">The connection to use</param>
        /// <param name="sql">The SQL</param>
        public SqlExecuter(MySqlConnection connection, string sql)
        {
            Sql = sql;
            Connection = connection;
            ConnectionString = connection.ConnectionString;
            DisoposeConnection = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">The string for creating a connection</param>
        /// <param name="sql">The SQL</param>
        /// <param name="commandTimeout">The number of seconds before an execution timesout and is canceled</param>
        public SqlExecuter(string connectionString, string sql, int commandTimeout = 30)
        {
            ConnectionString = connectionString;
            Sql = sql;
            CommandTimeout = commandTimeout;

            Connection = new MySqlConnection(connectionString);
            DisoposeConnection = true;
        }

        /// <summary>
        /// If called the executer will create it's own transaction
        /// </summary>
        /// <returns></returns>
        public SqlExecuter SetUseTransaction()
        {
            UseTransaction = true;
            return this;
        }

        /// <summary>
        /// Takes and object and reads all of the properties and creates sql parameters for them
        /// </summary>
        /// <param name="o">The object to read the parameters from</param>
        /// <param name="exclude">Ignored these properties (is case sensitive)</param>
        /// <returns>The executer</returns>
        public SqlExecuter SetParametersByObjectProperties(object o, params string[] exclude)
        {
            Func<object, object> fix = x =>
             {
                 if (x == null)
                     return x;

                 Type t = x.GetType();

                 // handle enumerations
                 if (t.IsEnum)
                 {
                     return x.ToString();
                 }

                 // handle sets
                 if (t.IsArray && t.GetElementType() == typeof(string))
                 {
                     return string.Join(",", (string[])x);
                 }

                 return x;
             };

            Type t = o.GetType();
            var parameters = t.GetProperties()
              .Where(p => !exclude.Contains(p.Name))
              .Select(p => new MySqlParameter($"@{p.Name}", fix(p.GetValue(o))));

            if (Parameters == null)
            {
                Parameters = parameters.ToArray();
            }
            else
            {
                Parameters = parameters.Union(Parameters).ToArray();
            }

            return this;
        }

        /// <summary>
        /// Clears the parameters
        /// </summary>
        /// <returns></returns>
        public SqlExecuter ClearParameters()
        {
            Parameters = null;
            return this;
        }

        /// <summary>
        /// Manually sets the parameters
        /// </summary>
        /// <param name="parameters">The parameters to set</param>
        /// <returns></returns>
        public SqlExecuter SetParmeters(params IDbDataParameter[] parameters)
        {
            if (Parameters == null)
            {
                Parameters = parameters.ToArray();
            }
            else
            {
                Parameters = parameters.Union(Parameters).ToArray();
            }

            return this;
        }

        /// <summary>
        /// Add a parameter to the query
        /// </summary>
        /// <param name="name">The name of the parameter.  Example: @Id</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>The executer instance</returns>
        public SqlExecuter SetParameter(string name, object value)
        {
            var p = new IDbDataParameter[] { new MySqlParameter(name, value) };

            if (Parameters == null)
            {
                Parameters = p;
            }
            else
            {
                Parameters = Parameters.Union(p).ToArray();
            }

            return this;
        }

        /// <summary>
        /// Executes the sql query as specified.
        /// </summary>
        /// <param name="exec">The specific method for execution</param>
        private void Execute(Action<IDbCommand> exec)
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }

                IDbTransaction tran = null;
                try
                {
                    if (UseTransaction)
                    {
                        tran = Connection.BeginTransaction();
                    }

                    var cmd = Connection.CreateCommand();
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.CommandText = Sql;
                    if (Parameters != null && Parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(Parameters);
                    }

                    exec(cmd);

                    if (UseTransaction)
                    {
                        tran.Commit();
                    }
                }
                catch (Exception)
                {
                    if (UseTransaction)
                    {
                        tran.Rollback();
                    }

                    throw;
                }
            }
            finally
            {
                if (DisoposeConnection)
                {
                    Connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Executes a query that doesn't expect a result
        /// </summary>
        public void ExecuteNonQuery()
        {
            Execute(cmd =>
            {
                cmd.ExecuteNonQuery();
            });
        }

        /// <summary>
        /// Executes a query that doesn't expect a result
        /// </summary>
        /// <param name="lastId">The first record's item</param>
        /// <param name="rowCount">The number of rows counted</param>
        public void ExecuteNonQuery(out long lastId, out long rowCount)
        {
            long id = 0;
            long rc = 0;
            Execute(cmd =>
            {
                rc = cmd.ExecuteNonQuery();
                MySqlCommand c = (cmd as MySqlCommand);
                id = c.LastInsertedId;
            });
            lastId = id;
            rowCount = rc;
        }


        /// <summary>
        /// Executes a query that expects a single scalar result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ExecuteScalar<T>()
        {
            T result = default(T);
            Execute(cmd =>
            {
                result = (T)cmd.ExecuteScalar();
            });

            return result;
        }

        /// <summary>
        /// Executes a command that expects to read rows with the row reader
        /// </summary>
        /// <param name="handler">The action that actually reads the data returned</param>
        public void ExecuteReader(Action<IDataReader> handler)
        {
            Execute(cmd =>
            {
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    handler(reader);
                }
            });
        }

        /// <summary>
        /// Executes a query and creates an enumerator to read the objects from the reader
        /// </summary>
        /// <typeparam name="T">The object type to be returned</typeparam>
        /// <param name="readObject">The function that reads the object from a reader</param>
        /// <param name="commandTimeout">The number of seconds that the command is allowed to execute</param>
        /// <returns>The inumerable</returns>
        public IEnumerable<T> ExecuteEnumerator<T>(Func<IDataReader, T> readObject, int commandTimeout = 30)
        {
            try
            {
                Connection.Open();

                var cmd = Connection.CreateCommand();
                cmd.CommandTimeout = commandTimeout;
                cmd.CommandText = Sql;
                if (Parameters != null && Parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(Parameters);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T item = readObject(reader);
                        yield return item;
                    }
                }
            }
            finally
            {
                if (DisoposeConnection)
                    Connection.Dispose();
            }
        }
    }
}
