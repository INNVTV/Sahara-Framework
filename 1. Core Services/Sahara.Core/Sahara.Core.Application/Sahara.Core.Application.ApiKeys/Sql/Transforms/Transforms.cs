using Sahara.Core.Application.ApiKeys.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sahara.Core.Application.ApiKeys.Sql
{
    public static class Transforms
    {
        public static ApiKeyModel DataReader_to_ApiKeyModel(SqlDataReader reader)
        {
            ApiKeyModel key = new ApiKeyModel();

            key.ApiKey = (Guid)reader["ApiKey"];
            key.Name = (String)reader["Name"];
            key.Description = (String)reader["Description"];
            key.CreatedDate = (DateTime)reader["CreatedDate"];

            return key;
        }
    }
}
