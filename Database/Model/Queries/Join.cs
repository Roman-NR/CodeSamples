using System.Text;

namespace CodeSamples.Database.Queries
{
    internal class Join
    {
        private string _table;
        private bool _outer;
        private TermCollection _on = new TermCollection();

        public Join(string table, bool outer)
        {
            _table = table;
            _outer = outer;
        }

        public string Table
        {
            get { return _table; }
        }

        public TermCollection On
        {
            get { return _on; }
        }

        public void AppendJoinText(StringBuilder query)
        {
            if (_outer)
                query.Append("LEFT OUTER JOIN ");
            else
                query.Append("INNER JOIN ");

            query.Append(_table);

            _on.AppendTermText(query, "ON", false);
        }
        
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            AppendJoinText(result);
            return result.ToString();
        }
    }
}
