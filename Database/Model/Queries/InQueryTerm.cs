using System.Text;

namespace CodeSamples.Database.Queries
{
    internal class InQueryTerm : Term
    {
        private string _field;
        private string _query;
        private bool _not;

        public InQueryTerm(string field, string query, bool not)
        {
            _field = field;
            _query = query;
            _not = not;
        }

        public override bool IsEmpty()
        {
            return string.IsNullOrEmpty(_query);
        }

        public override void AppendTermText(StringBuilder query)
        {
            query.Append(_field);
            if (_not)
                query.Append(" NOT");
            query.Append(" IN (");
            query.Append(_query);
            query.Append(")");
        }
    }
}
