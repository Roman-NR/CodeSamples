using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
/* -- */

namespace CodeSamples.Database.Queries
{
    internal class TemporaryTable : IEnumerable<string>
    {
        private string _name;
        private List<string> _fields = new List<string>();
        private SelectQuery _fillQuery;

        public TemporaryTable(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public void Add(string field, string type, bool nullable = true, bool primaryKey = false)
        {
            _fields.Add(string.Concat(
                field.Field(), " ", type, 
                nullable ? " NULL" : " NOT NULL", 
                primaryKey ? " PRIMARY KEY" : string.Empty));
        }

        public void Add(string field, DataType type, bool nullable = true, bool primaryKey = false)
        {
            Add(field, type.GetFieldTypeString(null), nullable, primaryKey);
        }

        public void Add(SystemColumnType field, bool nullable = false, bool primaryKey = false)
        {
            Add(field.FieldName(), field.GetDataType(), nullable, primaryKey);
        }

        public SelectQuery FillQuery
        {
            get { return _fillQuery; }
            set { _fillQuery = value; }
        }

        public void CreateTable(DatabaseConnection database, bool dropExisting = false)
        {
            if (dropExisting)
                DropTable(database);
            StringBuilder query = new StringBuilder();
            query.Append("CREATE TABLE [").Append(_name).AppendLine("] (");
            for (int i = 0; i < _fields.Count; ++i)
            {
                if (i > 0)
                    query.AppendLine(", ");
                query.Append(_fields[i]);
            }
            query.Append(")");

            database.ExecuteNonQuery(query.ToString());
        }

        public void DropTable(DatabaseConnection database)
        {
            StringBuilder query = new StringBuilder();
            query.Append("IF OBJECT_ID(N'tempdb..[").Append(_name).AppendLine("]') IS NOT NULL");
            query.Append("DROP TABLE [").Append(_name).AppendLine("];");
            database.ExecuteNonQuery(query.ToString());
        }

        public void AppendQueryText(StringBuilder query)
        {
            query.Append("DECLARE ").Append(_name).Append(" TABLE (").Append(string.Join(", ", _fields)).AppendLine(");");
            if (_fillQuery != null)
            {
                if (!string.IsNullOrEmpty(_fillQuery.Cte))
                    query.AppendLine(_fillQuery.Cte);
                query.Append("INSERT INTO ").AppendLine(_name);
                _fillQuery.AppendQueryText(query, false);
                query.AppendLine(";");
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            AppendQueryText(result);
            return result.ToString();
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
