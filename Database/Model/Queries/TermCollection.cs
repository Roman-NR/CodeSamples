using System;
using System.Text;

namespace CodeSamples.Database.Queries
{
    internal class TermCollection : Term
    {
        private static string[] _operatorNames = new string[] { " AND ", " OR " };

        private TermItem _firstTerm;
        private TermItem _lastTerm;
        private bool _not;

        public TermCollection()
        {
        }

        public bool Not
        {
            get { return _not; }
            set { _not = value; }
        }

        public TermCollection And(Term term)
        {
            if (term == null)
                return this;

            AddTerm(term, TermOperator.And);
            return this;
        }

        public TermCollection Or(Term term)
        {
            if (term == null)
                return this;

            AddTerm(term, TermOperator.Or);
            return this;
        }

        private void AddTerm(Term term, TermOperator @operator)
        {
            TermItem item = new TermItem() { Operator = @operator, Term = term };
            if (_firstTerm == null)
                _firstTerm = _lastTerm = item;
            else
            {
                _lastTerm.NextItem = item;
                _lastTerm = _lastTerm.NextItem;
            }
        }

        public override void AppendTermText(StringBuilder query)
        {
            AppendTermText(query, null, false);
        }

        public bool AppendTermText(StringBuilder query, string keyword, bool newLine)
        {
            if (keyword != null && IsEmpty()) // IsEmpty() заполняет TermItem.IsEmpty
                return false;

            if (newLine)
                query.AppendLine();

            if (keyword != null)
                query.Append(' ').Append(keyword).Append(' ');

            if (_not)
                query.Append("NOT ");

            if ((keyword == null && _firstTerm.NextItem != null) || _not)
                query.Append('(');

            for (TermItem term = _firstTerm; term != null; term = term.NextItem)
            {
                if (term.IsEmpty)
                    continue;

                if (term != _firstTerm)
                    query.Append(_operatorNames[(int)term.Operator]);
                term.Term.AppendTermText(query);
            }

            if ((keyword == null && _firstTerm.NextItem != null) || _not)
                query.Append(')');

            return true;
        }

        public override bool IsEmpty()
        {
            bool result = true;
            for (TermItem term = _firstTerm; term != null; term = term.NextItem)
            {
                term.IsEmpty = term.Term.IsEmpty();
                if (!term.IsEmpty)
                    result = false;
            }
            return result;
        }

        public void Clear()
        {
            _firstTerm = null;
            _lastTerm = null;
        }

        public override string ToString()
        {
            if (IsEmpty())
                return string.Empty;            
            return base.ToString();
        }

        private class TermItem
        {
            public TermOperator Operator;
            public Term Term;
            public TermItem NextItem;
            public bool IsEmpty;
        }
    }

    internal enum TermOperator
    {
        And,
        Or
    }
}
