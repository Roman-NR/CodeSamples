using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
/* -- */

namespace CodeSamples.Database.Queries
{
    internal class InTerm<T> : Term
    {
        private string _field;
        private bool _not;
        private IList<string> _values;

        public InTerm(string field, IEnumerable<T> values, DataType type, bool not, ICollection<SqlParameter> parameters, PropertyData property)
        {
            _field = field;
            _not = not;
            _values = type.EscapeList(values, property, parameters);
        }

        public override bool IsEmpty()
        {
            return _values.Count == 0;
        }

        public override void AppendTermText(StringBuilder query)
        {
            query.Append(_field);
            if (_not)
                query.Append(" NOT");
            query.Append(" IN (");
            for (int i = 0; i < _values.Count; ++i)
            {
                if (i > 0)
                    query.Append(", ");
                query.Append(_values[i]);
            }
            query.Append(')');
        }
    }
}
