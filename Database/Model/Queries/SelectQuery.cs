using System.Collections.Generic;
using System.Text;
/* -- */

namespace CodeSamples.Database.Queries
{
    internal class SelectQuery : Query
    {
        private string _table;
        private bool _distinct;
        private int _top;
        private List<string> _fields = new List<string>(20);
        private List<Join> _joins = new List<Join>(3);
        private TermCollection _where = new TermCollection();
        private List<string> _orders = new List<string>(2);
        private List<string> _groupBy = new List<string>(2);
        private string _cte;

        public SelectQuery()
        {
        }

        public SelectQuery(string table)
        {
            _table = table;
        }

        public string Table
        {
            get { return _table; }
            set { _table = value; }
        }

        public bool Distinct
        {
            get { return _distinct; }
            set { _distinct = value; }
        }

        public int Top
        {
            get { return _top; }
            set { _top = value; }
        }

        public string Cte
        {
            get { return _cte; }
            set { _cte = value; }
        }

        public IList<string> Fields
        {
            get { return _fields; }
        }

        public void AddField(string field)
        {
            _fields.Add(field);
        }

        public void AddField(string field, string alias)
        {
            if (string.IsNullOrEmpty(alias))
                AddField(field);
            else
                _fields.Add(string.Concat(field, " AS ", alias));
        }

        public void AddFields(params string[] fields)
        {
            if (fields == null)
                return;
            _fields.AddRange(fields);
        }

        public void AddCountField(string fieldName)
        {
            _fields.Add(string.Concat("COUNT(", fieldName, ")"));
        }

        public Join InnerJoin(string table)
        {
            Join result = new Join(table, false);
            _joins.Add(result);
            return result;
        }

        public Join LeftOuterJoin(string table)
        {
            Join result = new Join(table, true);
            _joins.Add(result);
            return result;
        }

        public Join FindJoin(string table)
        {
            return _joins.Find(j => j.Table == table);
        }

        public TermCollection Where
        {
            get { return _where; }
        }

        public void OrderBy(string field)
        {
            _orders.Add(field);
        }

        public void OrderByAsc(string field)
        {
            _orders.Add(field + " ASC");
        }

        public void OrderByDesc(string field)
        {
            _orders.Add(field + " DESC");
        }

        public void GroupBy(string field)
        {
            _groupBy.Add(field);
        }

        public override void AppendQueryText(StringBuilder query)
        {
            AppendQueryText(query, true);
        }

        public void AppendQueryText(StringBuilder query, bool appendCte)
        {
            base.AppendQueryText(query);

            if (appendCte && !string.IsNullOrEmpty(_cte))
                query.AppendLine(_cte);

            query.Append("SELECT ");

            if (_distinct)
                query.Append("DISTINCT ");

            if (_top > 0)
                query.Append("TOP (").Append(_top).Append(") ");

            if (_fields.Count == 0)
                query.Append('*');
            else
                query.Append(_fields, ", ");

            if (string.IsNullOrEmpty(_table))
                return;

            query.AppendLine().Append(" FROM ").Append(_table);

            foreach (Join join in _joins)
            {
                query.AppendLine();
                join.AppendJoinText(query);
            }

            _where.AppendTermText(query, "WHERE", true);

            if (_groupBy.Count > 0)
                query.AppendLine().Append("GROUP BY ").Append(_groupBy, ", ");

            if (_orders.Count > 0)
                query.AppendLine().Append(" ORDER BY ").Append(_orders, ", ");
        }
    }
}
