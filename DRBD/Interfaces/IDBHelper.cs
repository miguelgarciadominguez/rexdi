using Microsoft.Data.SqlClient;

namespace DRBD.Interfaces
{
    public interface IDBHelper : IDisposable
    {
        Task<SqlDataReader> DoQueryAsync(string query, List<DBParameter>? parameters);
        Task<int> ExecuteNonQueryAsync(string query, List<DBParameter>? parameters);
        Task<T?> ExecuteScalarAsync<T>(string query, List<DBParameter>? parameters);
    }
}