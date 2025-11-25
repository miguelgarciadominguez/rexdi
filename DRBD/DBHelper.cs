using DRBD.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DRBD
{
    public class DBHelper : IDBHelper
    {
        private readonly IConfiguration _configuration;

        public DBHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<SqlDataReader> DoQueryAsync(string query, List<DBParameter>? parameters)
        {
            var stringConnection = _configuration.GetConnectionString("DefaultConnection");
            var connection = new SqlConnection(stringConnection);
            try
            {
                await connection.OpenAsync();
                using var command = new SqlCommand(query, connection);
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Name, param.Value);
                    }
                }
                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            catch
            {
                connection?.Dispose();
                throw;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, List<DBParameter>? parameters)
        {
            var stringConnection = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(stringConnection);
            await connection.OpenAsync();
            using var command = new SqlCommand(query, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Name, param.Value);
                }
            }
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T?> ExecuteScalarAsync<T>(string query, List<DBParameter>? parameters)
        {
            var stringConnection = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(stringConnection);
            await connection.OpenAsync();
            using var command = new SqlCommand(query, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Name, param.Value);
                }
            }
            var result = await command.ExecuteScalarAsync();
            return result is T value ? value : default(T);
        }

        public void Dispose()
        {

        }
    }
}
