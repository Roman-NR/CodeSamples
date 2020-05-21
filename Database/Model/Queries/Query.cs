using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace CodeSamples.Database.Queries
{
    internal abstract class Query : ICollection<SqlParameter>
    {
        private ICollection<SqlParameter> _parameters;
        private List<TemporaryTable> _tables;
        private int _timeout = -1;

        protected Query(ICollection<SqlParameter> parameters = null)
        {
            _parameters = parameters;
        }

        public int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public int ExecuteNonQuery(DatabaseConnection database)
        {
            return database.ExecuteNonQuery(GetText(), GetCommandType(), _timeout, GetParameters());
        }

        public object ExecuteScalar(DatabaseConnection database)
        {
            return database.ExecuteScalar(GetText(), GetCommandType(), _timeout, GetParameters());
        }

        public bool ExecuteScalar<TResult>(DatabaseConnection database, out TResult result)
        {
            return database.ExecuteScalar<TResult>(GetText(), GetCommandType(), _timeout, out result, GetParameters()); 
        }

        public SqlDataReader ExecuteReader(DatabaseConnection database)
        {
            return database.ExecuteReader(GetText(), GetCommandType(), _timeout, GetParameters());
        }

        public DataTable ReadDataTable(DatabaseConnection database)
        {
            return database.ReadDataTable(GetText(), GetCommandType(), _timeout, GetParameters());
        }

        public virtual void AppendQueryText(StringBuilder query)
        {
            if (_tables != null)
            {
                foreach (TemporaryTable table in _tables)
                    table.AppendQueryText(query);
            }
        }

        protected virtual CommandType GetCommandType()
        {
            return CommandType.Text;
        }

        private string GetText()
        {
            StringBuilder result = new StringBuilder();
            AppendQueryText(result);
            return result.ToString();
        }

        public override string ToString()
        {
            return GetText();
        }

        public virtual void SetParent(Query parentQuery)
        {
            if (_parameters != null)
            {
                if (parentQuery._parameters == null)
                    parentQuery._parameters = _parameters;
                else
                {
                    foreach (SqlParameter parameter in _parameters)
                        parentQuery._parameters.Add(parameter);
                }
                _parameters = null;
            }
            if (_tables != null)
            {
                if (parentQuery._tables == null)
                    parentQuery._tables = _tables;
                else
                {
                    foreach (TemporaryTable table in _tables)
                        parentQuery._tables.Add(table);
                }
                _tables = null;
            }
        }

        private SqlParameter[] GetParameters()
        {
            return _parameters == null ? null : _parameters.ToArray();
        }

        public void AddParameter(SqlParameter parameter)
        {
            if (_parameters == null)
                _parameters = new List<SqlParameter>();
            _parameters.Add(parameter);
        }

        public void AddTable(TemporaryTable table)
        {
            if (_tables == null)
                _tables = new List<TemporaryTable>();
            _tables.Add(table);
        }

        #region ICollection<SqlParameter> Members

        void ICollection<SqlParameter>.Add(SqlParameter item)
        {
            AddParameter(item);
        }

        void ICollection<SqlParameter>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<SqlParameter>.Contains(SqlParameter item)
        {
            return _parameters != null && _parameters.Contains(item);
        }

        void ICollection<SqlParameter>.CopyTo(SqlParameter[] array, int arrayIndex)
        {
            if (_parameters != null)
                _parameters.CopyTo(array, arrayIndex);
        }

        int ICollection<SqlParameter>.Count
        {
            get { return _parameters == null ? 0 : _parameters.Count; }
        }

        bool ICollection<SqlParameter>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<SqlParameter>.Remove(SqlParameter item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<SqlParameter> Members

        IEnumerator<SqlParameter> IEnumerable<SqlParameter>.GetEnumerator()
        {
            return (_parameters == null ? new SqlParameter[0] : _parameters).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_parameters == null ? new SqlParameter[0] : _parameters).GetEnumerator();
        }

        #endregion
    }
}
