using System.Text;

namespace CodeSamples.Database.Queries
{
    internal class SimpleTerm : Term
    {
        private string _firstOperand;
        private string _operator;
        private string _secondOperand;

        public SimpleTerm(string firstOperand, string @operator, string secondOperand)
        {
            _firstOperand = firstOperand;
            _operator = @operator;
            _secondOperand = secondOperand;
        }

        public override bool IsEmpty()
        {
            return false;
        }

        public override void AppendTermText(StringBuilder query)
        {
            query.Append(_firstOperand);
            query.Append(' ').Append(_operator).Append(' ');
            query.Append(_secondOperand);
        }
    }
}
