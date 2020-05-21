using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CodeSamples.Database
{
    internal class DatabaseConnection : IDisposable, ICloneable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private int _openCounter;
        private int _transactionCounter;
        private int _commandTimeout = 150; // 2.5 минуты

        public DatabaseConnection(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        #endregion

        public int CommandTimeout
        {
            get { return _commandTimeout; }
            set { _commandTimeout = value; }
        }

        public ConnectionSession Open()
        {
            if (++_openCounter == 1)
                _connection.Open();
            return new ConnectionSession(this);
        }

        public void Close()
        {
            if (_openCounter == 0)
                return;

            if (--_openCounter == 0)
                _connection.Close();
        }

        public void BeginTransaction(string name)
        {
            if (++_transactionCounter == 1)
                _transaction = _connection.BeginTransaction(name);
            else
                _transaction.Save(name);
        }

        public void CommitTransaction()
        {
            if (--_transactionCounter > 0)
                return;

            using (SqlTransaction transaction = _transaction)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }

        public void RollbackTransaction(string name)
        {
            if (--_transactionCounter > 0)
                _transaction.Rollback(name);
            else
            {
                using (SqlTransaction transaction = _transaction)
                {
                    try
                    {
                        _transaction.Rollback();
                    }
                    catch (InvalidOperationException)
                    {
                        // The transaction has already been committed or rolled back. 
                        // -or- 
                        // The connection is broken.
                    }
                    _transaction = null;
                }
            }
        }

        public int ExecuteNonQuery(string cmdText, params SqlParameter[] parameters)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, -1, parameters);
        }

        public int ExecuteNonQuery(string cmdText, int timeout, params SqlParameter[] parameters)
        {
            return ExecuteNonQuery(cmdText, CommandType.Text, timeout, parameters);
        }

        public int ExecuteNonQuery(string cmdText, CommandType type, int timeout, params SqlParameter[] parameters)
        {
            SqlCommand command = CreateCommand(cmdText, type, timeout, parameters);
            using (QueryProfiler profiler = new QueryProfiler(cmdText, parameters))
            {
                try
                {
                    return command.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    throw new DatabaseException(QueryProfiler.ReplaceParameters(cmdText, parameters), e);
                }
            }
        }

        public object ExecuteScalar(string cmdText, params SqlParameter[] parameters)
        {
            return ExecuteScalar(cmdText, CommandType.Text, -1, parameters);
        }

        public object ExecuteScalar(string cmdText, CommandType type, int timeout, params SqlParameter[] parameters)
        {
            SqlCommand command = CreateCommand(cmdText, type, timeout, parameters);
            using (QueryProfiler profiler = new QueryProfiler(cmdText, parameters))
            {
                try
                {
                    return command.ExecuteScalar();
                }
                catch (SqlException e)
                {
                    throw new DatabaseException(QueryProfiler.ReplaceParameters(cmdText, parameters), e);
                }
            }
        }

        public bool ExecuteScalar<TResult>(string cmdText, out TResult result, params SqlParameter[] parameters)
        {
            return ExecuteScalar<TResult>(cmdText, CommandType.Text, -1, out result, parameters);
        }

        public bool ExecuteScalar<TResult>(string cmdText, CommandType type, int timeout, out TResult result, params SqlParameter[] parameters)
        {
            object value = ExecuteScalar(cmdText, type, timeout, parameters);
            if (value == null)
            {
                result = default(TResult);
                return false;
            }

            if (Convert.IsDBNull(value))
                value = null;

            if (value is TResult)
                result = (TResult)value;
            else
                result = (TResult)Convert.ChangeType(value, typeof(TResult));

            return true;
        }

        public SqlDataReader ExecuteReader(string cmdText, params SqlParameter[] parameters)
        {
            return ExecuteReader(cmdText, CommandType.Text, -1, parameters);
        }

        public SqlDataReader ExecuteReader(string cmdText, CommandType type, int timeout, params SqlParameter[] parameters)
        {
            SqlCommand command = CreateCommand(cmdText, type, timeout, parameters);
            using (QueryProfiler profiler = new QueryProfiler(cmdText, parameters))
            {
                try
                {
                    return command.ExecuteReader();
                }
                catch (SqlException e)
                {
                    throw new DatabaseException(QueryProfiler.ReplaceParameters(cmdText, parameters), e);
                }
            }
        }
        public DataTable ReadDataTable(string cmdText, CommandType type, int timeout, params SqlParameter[] parameters)
        {
            SqlCommand command = CreateCommand(cmdText, type, timeout, parameters);
            using (QueryProfiler profiler = new QueryProfiler(cmdText, parameters))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    try
                    {
                        DataTable result = new DataTable();
                        adapter.Fill(result);
                        return result;
                    }
                    catch (SqlException e)
                    {
                        throw new DatabaseException(QueryProfiler.ReplaceParameters(cmdText, parameters), e);
                    }
                }
            }
        }

        public SqlCommand CreateCommand(string cmdText, CommandType type, int timeout, params SqlParameter[] paramters)
        {
            SqlCommand result = new SqlCommand(cmdText, _connection, _transaction);
            result.CommandTimeout = timeout < 0 ? _commandTimeout : timeout;
            if (paramters != null)
                result.Parameters.AddRange(paramters);
            result.Prepare();
            return result;
        }

        public bool TableExists(string tableName)
        {
            return ObjectExists(tableName);
        }

        public bool ViewExists(string viewName)
        {
            return ObjectExists(viewName);
        }

        public bool FunctionExists(string functionName)
        {
            return ObjectExists(functionName);
        }

        private bool ObjectExists(string name)
        {
            using (ConnectionSession connection = Open())
                return !Convert.IsDBNull(ExecuteScalar(string.Format("SELECT OBJECT_ID('[dbo].[{0}]')", name.Replace("'", "''"))));
        }

        public bool FieldExists(string tableName, string fieldName, string condition = null)
        {
            string query = "SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[{0}]') AND name = @name";
            if (!string.IsNullOrEmpty(condition))
                query += " AND " + condition;

            return null != ExecuteScalar(
                string.Format(query, tableName.Replace("'", "''")),
                new SqlParameter("@name", fieldName) { SqlDbType = SqlDbType.NVarChar, Size = int.MaxValue, IsNullable = false });
        }

        public bool IndexExists(string tableName, string indexName)
        {
            return null != ExecuteScalar(
                string.Format("SELECT index_id FROM sys.indexes WHERE object_id = OBJECT_ID('[dbo].[{0}]') AND name = @name", tableName.Replace("'", "''")),
                new SqlParameter("@name", indexName) { SqlDbType = SqlDbType.NVarChar, Size = int.MaxValue, IsNullable = false });
        }

        public void ClearFieldConstraints(string tableName, string fieldName)
        {
            tableName = tableName.Replace("'", "''");
            fieldName = fieldName.Replace("'", "''");

            string query = string.Format(
@"SELECT dc.name FROM sys.default_constraints dc
	INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
 WHERE dc.parent_object_id = OBJECT_ID('[dbo].[{0}]') AND c.name = N'{1}'", tableName, fieldName);

            string constraintName;
            if (ExecuteScalar<string>(query, out constraintName))
                ExecuteNonQuery(string.Format("ALTER TABLE [dbo].[{0}] DROP CONSTRAINT [{1}]", tableName, constraintName));

            query = string.Format(
@"SELECT i.name FROM sys.indexes i
	INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
	INNER JOIN sys.columns c ON c.object_id = i.object_id AND c.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID('[dbo].[{0}]') AND c.name = N'{1}'", tableName, fieldName);

            List<string> indexNames = new List<string>();
            using (SqlDataReader reader = ExecuteReader(query))
                while (reader.Read())
                    indexNames.Add(reader.GetString(0));

            foreach (string indexName in indexNames)
                ExecuteNonQuery(string.Format("DROP INDEX [{0}] ON [dbo].[{1}]", indexName, tableName));
        }

        public string GetExtendedPropertyValue(string propertyName)
        {
            string query = "SELECT value FROM sys.fn_listextendedproperty(@name, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT)";
            return ExecuteScalar(query, out string value, 
                new SqlParameter("@name", propertyName) { SqlDbType = SqlDbType.NVarChar, Size = 255, IsNullable = false }
                ) ? value : null;
        }

        public void SetExtendedPropertyValue(string propertyName, string value)
        {
            string query =
@"IF NOT EXISTS (SELECT 1 FROM sys.fn_listextendedproperty(@name, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT, DEFAULT))
	EXEC sys.sp_addextendedproperty @name, @value
ELSE
	EXEC sys.sp_updateextendedproperty @name, @value";

            ExecuteNonQuery(query,
                new SqlParameter("@name", propertyName) { SqlDbType = SqlDbType.NVarChar, Size = 255, IsNullable = false }, 
                new SqlParameter("@value", value) { SqlDbType = SqlDbType.NVarChar, Size = 255, IsNullable = false });
        }

        public void RenameTable(string currentName, string newName)
        {
            ExecuteNonQuery("EXEC sp_rename @currentName, @newName",
                new SqlParameter("@currentName", $"[dbo].[{currentName}]") { SqlDbType = SqlDbType.NVarChar, Size = 255, IsNullable = false },
                new SqlParameter("@newName", newName) { SqlDbType = SqlDbType.NVarChar, Size = 255, IsNullable = false });
        }

        public void RenameField(string tableName, string currentName, string newName)
        {
            ExecuteNonQuery("EXEC sp_rename @currentName, @newName, 'COLUMN'",
                new SqlParameter("@currentName", $"[dbo].[{tableName}].[{currentName}]") { SqlDbType = SqlDbType.NVarChar, Size = 255, IsNullable = false },
                new SqlParameter("@newName", newName) { SqlDbType = SqlDbType.NVarChar, Size = 255, IsNullable = false });
        }

        public SqlBulkCopy CreateBulkCopy(int? timeout = null)
        {
            SqlBulkCopy result = new SqlBulkCopy(_connection, SqlBulkCopyOptions.Default, _transaction);
            if (timeout.HasValue)
                result.BulkCopyTimeout = Math.Max(result.BulkCopyTimeout, timeout.Value);
            return result;
        }

        #region ICloneable Members

        public DatabaseConnection Clone()
        {
            if (_connection == null)
                throw new ObjectDisposedException(GetType().FullName);

            return new DatabaseConnection(_connection.ConnectionString);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }
}
