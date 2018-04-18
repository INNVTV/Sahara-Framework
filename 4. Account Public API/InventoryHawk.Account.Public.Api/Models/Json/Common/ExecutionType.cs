using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryHawk.Account.Public.Api.Models.Json.Common
{
    public enum ExecutionType
    {
        local,
        redis_secondary,
        redis_main,
        wcf,
        searchIndex,
        documentDb
    }
}