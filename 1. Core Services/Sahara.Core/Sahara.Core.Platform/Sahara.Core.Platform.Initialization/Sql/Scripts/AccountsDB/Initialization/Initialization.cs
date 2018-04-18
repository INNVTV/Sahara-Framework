using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sahara.Core.Platform.Initialization.Sql.Scripts.AccountsDB.Initialization
{
    internal class Initialization
    {
        /// <summary>
        /// Used to run all .sql files inside of "Scripts/{Database/FolderName}/Initialization" folder against the database
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <returns></returns>
        public bool InitializeAccountsDB(ReliableSqlConnection sqlConnection)
        {
            var assembly = Assembly.GetAssembly(GetType());
            var assemblyName = assembly.GetName().Name;

            var resourceNames = assembly.GetManifestResourceNames();

            //List of script folders to be run (in order)
            var scriptsOrder = new List<string>();
            scriptsOrder.Add("Pre");
            scriptsOrder.Add("Tables");
            scriptsOrder.Add("Post");
            scriptsOrder.Add("Procedures");
            scriptsOrder.Add("Seed");


            //Loop through all scripts within each folder and run them against the database connection:
            //FYI: .sql Files must be saved as ANSI
            //FYI: .sql Files must be set as "Embedded Resource" & "CopyAlways" in Properties
            foreach (string folder in scriptsOrder)
            {
                foreach (var sqlScript in resourceNames.Where(o => o.StartsWith(assemblyName + ".Sql.Scripts.AccountsDB.Initialization." + folder)))
                {
                    using (var stream = assembly.GetManifestResourceStream(sqlScript))
                    using (var reader = new StreamReader(stream))
                    {
                        var split = SplitSqlStatements(reader.ReadToEnd());

                        foreach (var s in split)
                        {
                            executeNonQueryStatement(s, sqlConnection);
                        }
                    }
                }
            }

            return true;

        }

        private static void executeNonQueryStatement(string statement, ReliableSqlConnection sqlConnection)
        {
            if (statement != "")
            {

                //SqlCommand sqlCommandGenerateTables = new SqlCommand(statement, sqlConnection);
                SqlCommand sqlCommandGenerateTables = sqlConnection.CreateCommand();
			    sqlCommandGenerateTables.CommandText = statement;
			    

                try
                {

                    sqlCommandGenerateTables.Connection.OpenWithRetry();
                    sqlCommandGenerateTables.ExecuteNonQueryWithRetry();
                    sqlCommandGenerateTables.Connection.Close();
                }
                catch
                {
                    //Try again (ADO.NET Connection Pooling may require a retry)
                    sqlCommandGenerateTables.Connection.Close();

                    sqlCommandGenerateTables.Connection.OpenWithRetry();
                    sqlCommandGenerateTables.ExecuteNonQueryWithRetry();
                    sqlCommandGenerateTables.Connection.Close();
                }
            }

        }


        static IEnumerable<string> SplitSqlStatements(string sqlScript)
        {
            // Split by "GO" statements
            var statements = System.Text.RegularExpressions.Regex.Split(
                    sqlScript,
                    @"^\s*GO\s* ($ | \-\- .*$)",
                    System.Text.RegularExpressions.RegexOptions.Multiline |
                    System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace |
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\r', '\n'));
        }
    }
}
