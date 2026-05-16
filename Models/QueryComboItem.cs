namespace PosnaiSQLauncher.Models
{
    public class QueryComboItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}