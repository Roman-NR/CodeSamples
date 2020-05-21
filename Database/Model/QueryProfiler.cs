using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeSamples.Database
{
    internal class QueryProfiler : IDisposable
    {
#if DEBUG
        private static string _logFilePath;
        private static volatile uint _count = 0;

        static QueryProfiler()
        {
            _logFilePath = Path.Combine(Path.GetDirectoryName(typeof(QueryProfiler).Assembly.Location), "_Queries.log");
            File.Delete(_logFilePath);
        }

        private DateTime _startTime;
        private string _query;
        private uint _number;
#endif
        public QueryProfiler(string query, IEnumerable<SqlParameter> parameters)
        {
#if DEBUG
            query = ReplaceParameters(query, parameters);
            _startTime = DateTime.Now;
            _query = query;
            _number = _count++;
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
#if DEBUG
            TimeSpan time = DateTime.Now - _startTime;
            StringBuilder text = new StringBuilder();
            text.AppendFormat("Query #{0} at {1:G}", _number, _startTime).AppendLine();
            text.AppendLine(_query);
            text.AppendLine(string.Format("Cost: {0} s ", time.TotalSeconds).PadRight(100, '-')).AppendLine();
            lock (_logFilePath)
                File.AppendAllText(_logFilePath, text.ToString());
#endif
        }

        #endregion

        public static string ReplaceParameters(string query, IEnumerable<SqlParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (SqlParameter p in parameters.OrderByDescending(p => p.ParameterName))
                    query = query.Replace(p.ParameterName, string.Concat("N'", p.Value, "'"));
            }
            return query;
        }
    }
}
