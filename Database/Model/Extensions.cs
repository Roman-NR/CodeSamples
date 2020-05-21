using System;
using System.Data.SqlClient;

namespace CodeSamples.Database
{
    public static class Extensions
    {
        public static object GetNullableValue(this SqlDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetValue(i);
        }

        public static string GetNullableString(this SqlDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? null : reader.GetString(i);
        }

        public static int GetNullableInt32(this SqlDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? 0 : reader.GetInt32(i);
        }

        public static Guid? GetNullableGuid(this SqlDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (Guid?)null : reader.GetGuid(i);
        }

        public static DateTime? GetNullableDateTime(this SqlDataReader reader, int i)
        {
            return reader.IsDBNull(i) ? (DateTime?)null : reader.GetDateTime(i);
        }
    }
}
