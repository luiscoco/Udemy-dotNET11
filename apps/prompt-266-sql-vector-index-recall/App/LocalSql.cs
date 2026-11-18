using Microsoft.Data.SqlClient;
public static class LocalSql
{
    public static async Task<SqlConnection> Open(){var c=new SqlConnection("Server=127.0.0.1,14666;Database=CoursePreview266;User ID=sa;Password=Course266_Local!Only;Encrypt=True;TrustServerCertificate=True");await c.OpenAsync();return c;}
    public static async Task Execute(SqlConnection c,string sql){using var cmd=new SqlCommand(sql,c){CommandTimeout=120};await cmd.ExecuteNonQueryAsync();}
}
