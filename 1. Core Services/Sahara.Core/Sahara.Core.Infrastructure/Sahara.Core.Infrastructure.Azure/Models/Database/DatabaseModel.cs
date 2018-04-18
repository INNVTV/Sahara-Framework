using Sahara.Core.Infrastructure.Azure.Types.Database;

namespace Sahara.Core.Infrastructure.Azure.Models.Database
{

    public class DatabaseModel
    {
        public string DatabaseName { get; set; }
        public int DatabaseID { get; set; }
        public DatabaseTier DatabaseTier { get; set; }
        public string DatabaseSize { get; set; }
    }

}
