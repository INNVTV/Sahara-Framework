namespace Sahara.Core.Accounts.Sql.Statements
{
    public static class InsertStatements
    {

        /* Optional - for certain scenarios
        public static DataAccessResponseType InsertAccountUsersInAccounts(string accountId, string userId, bool isOwner)
        {
            DataAccessResponseType response = new DataAccessResponseType();
            response.isSuccess = false;

            StringBuilder SqlStatement = new StringBuilder();


            //newAccountModel.Provisioned = false;

            //SQL Statement =============================================================
            SqlStatement.Append("INSERT INTO AccountUsersInAccounts (");

            SqlStatement.Append("AccountID,");
            SqlStatement.Append("UserID,");
            SqlStatement.Append("Owner");


            SqlStatement.Append(") VALUES (");

            //Using parameterized queries to protect against injection
            SqlStatement.Append("@AccountID, ");
            SqlStatement.Append("@UserID, ");
            SqlStatement.Append("@Owner");

            SqlStatement.Append(")");

            SqlCommand sqlCommand = new SqlCommand(SqlStatement.ToString(), Sahara.Core.Settings.Azure.Databases.DatabaseConnections.AccountsSqlConnection);


            //Using parameterized queries to protect against injection
            sqlCommand.Parameters.Add("@AccountID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier);
            sqlCommand.Parameters.Add("@Owner", SqlDbType.Bit);




            sqlCommand.Parameters["@AccountID"].Value = accountId;
            sqlCommand.Parameters["@UserID"].Value = userId;
            sqlCommand.Parameters["@Owner"].Value = isOwner;


            int insertAccountResult = 0;

            sqlCommand.Connection.Open();

            try
            {
                insertAccountResult = sqlCommand.ExecuteNonQuery(); // returns Int indicating number of rows affected
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
                return response;
            }

            sqlCommand.Connection.Close();


            if (insertAccountResult == 1)
            {
                response.isSuccess = true;
            }
            else
            {
                response.ErrorMessage = "SQL result was malformed, check data integrity";
            }

            return response;
        }
         */

        
    }
}
