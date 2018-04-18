using Sahara.Core.Settings.Models.DataConnections;

namespace Sahara.Core.Settings.Azure
{
    public static class Databases
    {

        #region Private Properties

        private static DatabaseConnections _sqlDataConnections;

        #endregion

        #region INITIALIZE

        internal static void Initialize()
        {
            #region Environment Settings

            _sqlDataConnections = new DatabaseConnections();

            switch (Environment.Current.ToLower())
            {

                #region Production

                case "production":
                    _sqlDataConnections.SqlServerName = "[Config_SqlServerName]";
                    _sqlDataConnections.SqlUserName = "[Config_SqlUserName]";
                    _sqlDataConnections.SqlPassword = "[Config_SqlPassword]";
                    break;

                #endregion


                #region Stage

                case "stage":
                    _sqlDataConnections.SqlServerName = "[Config_SqlServerName]";
                    _sqlDataConnections.SqlUserName = "[Config_SqlUserName]";
                    _sqlDataConnections.SqlPassword = "[Config_SqlPassword]";
                    break;

                #endregion


                #region Local/Debug

                case "debug":
                    _sqlDataConnections.SqlServerName = "[Config_SqlServerName]";
                    _sqlDataConnections.SqlUserName = "[Config_SqlUserName]";
                    _sqlDataConnections.SqlPassword = "[Config_SqlPassword]";
                    break;

                case "local":
                    _sqlDataConnections.SqlServerName = "[Config_SqlServerName]";
                    _sqlDataConnections.SqlUserName = "[Config_SqlUserName]";
                    _sqlDataConnections.SqlPassword = "[Config_SqlPassword]";
                    break;

                    #endregion

            }

            #endregion
        }

        #endregion

        public static DatabaseConnections DatabaseConnections
        {
            get
            {
                return _sqlDataConnections;
            }

        }

    }
}
