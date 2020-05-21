using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
/* -- */

namespace CodeSamples.Database.Queries
{
    internal abstract class Term
    {
        public abstract bool IsEmpty();
        public abstract void AppendTermText(StringBuilder query);

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            AppendTermText(result);
            return result.ToString();
        }

        public static SimpleTerm Is<TFirst, TSecond>(TFirst firstOperand, string @operator, TSecond secondOperand)
        {
            return new SimpleTerm(firstOperand.ToString(), @operator, secondOperand.ToString());
        }
        
        public static InTerm<T> In<T>(string field, IEnumerable<T> values, ICollection<SqlParameter> parameters = null, PropertyData property = null)
        {
            return new InTerm<T>(field, values, DataType.FromType(typeof(T).Name), false, parameters, property);
        }

        public static InTerm<object> In(string field, IEnumerable<object> values, DataType type, ICollection<SqlParameter> parameters = null, PropertyData property = null)
        {
            return new InTerm<object>(field, values, type, false, parameters, property);
        }

        public static InTerm<T> NotIn<T>(string field, IEnumerable<T> values, ICollection<SqlParameter> parameters = null, PropertyData property = null)
        {
            return new InTerm<T>(field, values, DataType.FromType(typeof(T).Name), true, parameters, property);
        }

        public static InTerm<object> NotIn(string field, IEnumerable<object> values, DataType type, ICollection<SqlParameter> parameters = null, PropertyData property = null)
        {
            return new InTerm<object>(field, values, type, true, parameters, property);
        }

        public static InSelectTerm In(string field, SelectQuery select)
        {
            return new InSelectTerm(field, select, false);
        }

        public static InSelectTerm NotIn(string field, SelectQuery select)
        {
            return new InSelectTerm(field, select, true);
        }

        public static InQueryTerm In(string field, string query)
        {
            return new InQueryTerm(field, query, false);
        }

        public static InQueryTerm NotIn(string field, string query)
        {
            return new InQueryTerm(field, query, true);
        }
    }
}
