using System.Data.SqlClient;
/* -- */

namespace CodeSamples.Database
{
    internal class DatabaseException : ServerException
    {
        public DatabaseException(string query, SqlException innerException)
            : base(OperationErrorCode.DatabaseError, innerException, query)
        {
        }

        public string Query
        {
            get { return Data[0]; }
        }
    }
}
