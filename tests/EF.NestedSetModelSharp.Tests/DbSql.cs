using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EF.NestedSetModelSharp.Tests
{
    public class DbSql
    {
        public static void RunServerSql(string sql)
        {
            RunSql("Host=localhost;Port=4432;Database=test_nested;Username=postgres;Password=postgres", sql);
        }

        public static void RunDbSql(string sql)
        {
            RunSql(ConnectionString, sql);
        }

        public static string ConnectionString =>
            "Host=localhost;Port=4432;Database=test_nested;Username=postgres;Password=postgres";

        public static void RunSql(string connectionString, string sql)
        {
            // See: https://stackoverflow.com/a/18597011/64519
            // Allows usage of "GO" in executed statements
            using (var sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                using (var command = new SqlCommand())
                {
                    command.Connection = sqlConn;

                    var scripts = Regex.Split(sql, @"^GO\r?$", RegexOptions.Multiline);
                    foreach (var splitScript in scripts)
                    {
                        if (!string.IsNullOrWhiteSpace(splitScript))
                        {
                            command.CommandText = splitScript;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> SplitSqlStatements(string sqlScript)
        {
            // Split by "GO" statements
            var statements = Regex.Split(
                sqlScript,
                @"^[\t ]*GO[\t ]*\d*[\t ]*(?:--.*)?$",
                RegexOptions.Multiline |
                RegexOptions.IgnorePatternWhitespace |
                RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\r', '\n'));
        }
    }
}