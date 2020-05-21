using System.Collections.Generic;
using System.Text;

namespace CodeSamples.Database.Queries
{
    internal class UnionQuery : Query
    {
        public UnionQuery(bool all = true)
        {
            All = all;
            Selects = new List<SelectQuery>();
        }

        public bool All { get; set; }
        public List<SelectQuery> Selects { get; private set; }

        public override void AppendQueryText(StringBuilder query)
        {
            base.AppendQueryText(query);

            for (int i = 0; i < Selects.Count; ++i)
            {
                if (i > 0)
                    query.AppendLine().AppendLine(All ? "UNION ALL" : "UNION");
                Selects[i].AppendQueryText(query);
            }
        }
    }
}
