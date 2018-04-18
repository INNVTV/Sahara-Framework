using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace Sahara.Core.Infrastructure.Azure.Sql
{
    public static class ManagementStatements
    {

        public static bool CreateDatabase(string databaseName, SqlConnection sqlConnection)
        {
            bool response = false;

            //Drop databases (if exists)
            try
            {
                executeNonQueryStatement("Drop Database " + databaseName, sqlConnection);
            }
            catch
            {

            }


            //Create Database =============================================================
            executeNonQueryStatement("Create Database " + databaseName, sqlConnection);

            response = true;

            return response;
        }


        /// <summary>
        /// Used to run .sql files inside of "Scripts" folder against the database
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public static bool RunSqlScript(string filename, SqlConnection sqlConnection)
        {
            string[] StatementsArray;

            //FYI: .sql files inside of "Scripts" folder must be saved as ANSI
            //FYI: .sql files inside of "Scripts" folder must be set as "Embedded Resource" & "CopyAlways" in Properties


            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = Assembly.GetExecutingAssembly().GetName().Name + ".Sql.Scripts." + filename;
            string SqlGenerateScript;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                SqlGenerateScript = reader.ReadToEnd();
            }

            //REPLACE GO, Go & go statements with GO, then SPLIT and run seperatly:
            SqlGenerateScript = SqlGenerateScript.Replace("\r\nGo\r\n", "\r\nGO\r\n");
            SqlGenerateScript = SqlGenerateScript.Replace("\r\ngo\r\n", "\r\nGO\r\n");

            StatementsArray = System.Text.RegularExpressions.Regex.Split(SqlGenerateScript, "\r\nGO\r\n");
            foreach (string str in StatementsArray)
            {

                executeNonQueryStatement(str, sqlConnection);
            }

            StatementsArray = null;

            return true;
        }






        private static void executeNonQueryStatement(string statement, SqlConnection sqlConnection)
        {
            if (statement != "")
            {

                SqlCommand sqlCommand = new SqlCommand(statement, sqlConnection);

                try
                {

                    sqlCommand.Connection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Connection.Close();
                }
                catch
                {
                    //Try again (ADO.NET Connection Pooling may require a retry)
                    sqlCommand.Connection.Close();

                    sqlCommand.Connection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Connection.Close();
                }
            }

        }
    }
}
