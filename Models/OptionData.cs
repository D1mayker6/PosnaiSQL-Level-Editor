// Models/VariantData.cs
namespace PosnaiSQLauncher.Models
{
    public class OptionData
    {
        public int DatabaseId { get; set; }
        public string DatabaseName { get; set; }

        public int QueryId { get; set; }
        public string QueryName { get; set; }
        public string QueryCondition { get; set; }
        public string QueryString { get; set; }

        public int LocationId { get; set; }
        public string LocationName { get; set; }

        public int TimeLimit { get; set; } = 300;
        
        public string DatabaseSchemaImage { get; set; }
        
        public string DatabaseMode { get; set; }
        
        public string QueryMode { get; set; }

        public OptionData()
        {
        }

        public OptionData(int databaseId, string databaseName)
        {
            DatabaseId = databaseId;
            DatabaseName = databaseName;
        }
    }
}