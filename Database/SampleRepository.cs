using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/* -- */

namespace CodeSamples.Database
{
    internal class LogRepository : RepositoryBase, ILogRepository
    {
        public LogRepository(DatabaseConnection database)
            : base(new SystemOperationSession(database))
        {
        }

        public LogRepository(OperationSession session) 
            : base(session)
        {
        }

        public List<LogEntryData> GetEntries(LogEntryFilterData filter)
        {
            SelectQuery query = new SelectQuery("[dbo].[LogEntry] l");
            query.AddFields(
                "l.[ID]", "l.[Category]", "l.[SourceId]",
                "l.[ErrorLevel]", "l.[DateTime]", "l.[UserGuid]", "ISNULL(w.[HostName], N'')",
                "l.[ClientVersion]", "l.[ServerVersion]", "l.[Message]", "l.[Data]");
            query.Top = filter.MaxCount;
            query.LeftOuterJoin("[dbo].[Workspaces] w")
                .On.And(Term.Is("w.[ID]", "=", "l.[WorkspaceId]"));

            if (filter.MinId > 0)
                query.Where.And(Term.Is("l.[ID]", ">=", filter.MinId));
            if (filter.MaxId > 0)
                query.Where.And(Term.Is("l.[ID]", "<=", filter.MaxId));
            if (filter.MinDate.HasValue)
                query.Where.And(Term.Is("l.[DateTime]", ">=", DataType.DateTime.Escape(filter.MinDate.Value, parameters: query)));
            if (filter.MaxDate.HasValue)
                query.Where.And(Term.Is("l.[DateTime]", "<=", DataType.DateTime.Escape(filter.MaxDate.Value, parameters: query)));
            if (filter.Categories?.Length > 0)
                query.Where.And(Term.In("l.[Category]", filter.Categories));
            if (filter.ErrorLevels?.Length > 0)
                query.Where.And(Term.In("l.[ErrorLevel]", filter.ErrorLevels));
            if (filter.Ids?.Length > 0)
                query.Where.And(Term.In("l.[Id]", filter.Ids));
            if (filter.SourceIds?.Length > 0)
                query.Where.And(Term.In("l.[SourceId]", filter.SourceIds));
            if (filter.UserGuids?.Length > 0)
                query.Where.And(Term.In("l.[UserGuid]", filter.UserGuids));
            if (!string.IsNullOrWhiteSpace(filter.Text))
            {
                string text = DataType.String.Escape($"%{filter.Text}%", parameters: query);
                query.Where.And(new TermCollection()
                    .Or(Term.Is("l.[Message]", "LIKE", text))
                    .Or(Term.Is("l.[Data]", "LIKE", text))
                );
            }

            query.OrderByDesc("l.[ID]");

            List<LogEntryData> result = new List<LogEntryData>();
            using (Database.Open())
            {
                using (SqlDataReader reader = query.ExecuteReader(Database))
                {
                    while (reader.Read())
                    {
                        int field = 0;
                        result.Add(new LogEntryData
                        {
                            Id = reader.GetInt32(field++),
                            Category = reader.GetInt32(field++),
                            SourceId = reader.IsDBNull(field++) ? (Guid?)null : reader.GetGuid(field - 1),
                            ErrorLevel = reader.GetInt32(field++),
                            DateTime = reader.GetDateTime(field++),
                            UserGuid = reader.GetGuid(field++),
                            HostName = reader.GetString(field++),
                            ClientVersion = Version.TryParse(reader.GetString(field++), out Version version) ? version : null,
                            ServerVersion = Version.TryParse(reader.GetString(field++), out version) ? version : null,
                            Message = reader.GetString(field++),
                            Data = reader.GetString(field++)
                        });
                    }
                }
            }
            return result;
        }

        public int AddEntry(int category, int errorLevel, string message, 
                            string data = null, 
                            DateTime? dateTime = null, 
                            Guid? sourceId = null,
                            Guid? userGuid = null,
                            Version clientVersion = null)
        {
            using (Database.Open())
            {
                string query = $"INSERT INTO [dbo].[LogEntry] (" +
                    $"[Category], [SourceId], [ErrorLevel], " +
                    $"[DateTime], [UserGuid], [WorkspaceId], [ClientVersion], " +
                    $"[ServerVersion], [Message], [Data]) " +
                    $"VALUES (" +
                    $"{category}, {(sourceId.HasValue ? $"'{sourceId.Value}'" : "NULL")}, {errorLevel}, " +
                    $"@dateTime, '{userGuid ?? UserGuid}', {WorkspaceId}, N'{clientVersion}', " +
                    $"N'{Program.Version}', @message, @data); " +
                    $"SELECT SCOPE_IDENTITY();";

                Database.ExecuteScalar(query, out int id,
                    CreateParameter("@dateTime", dateTime ?? DateTime.Now),
                    CreateParameter("@message", message ?? string.Empty, 500),
                    CreateParameter("@data", data ?? string.Empty));
                return id;
            }
        }
    }
}
