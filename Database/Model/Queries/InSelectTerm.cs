using System.Text;

namespace CodeSamples.Database.Queries
{
    internal class InSelectTerm : Term
    {
        private string _field;
        private SelectQuery _select;
        private bool _not;

        public InSelectTerm(string field, SelectQuery select, bool not)
        {
            _field = field;
            _select = select;
            _not = not;
        }

        public override bool IsEmpty()
        {
            return false;
        }

        public override void AppendTermText(StringBuilder query)
        {
            query.Append(_field);
            if (_not)
                query.Append(" NOT");
            query.Append(" IN (");
            _select.AppendQueryText(query);
            query.Append(")");
        }
    }
}
