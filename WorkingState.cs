namespace PosnaiSQLauncher;

public class WorkingState
{
    public int? OptionId { get; set; }
    public int? LocationId { get; set; }
    public int? IdDatabase { get; set; }
    public int? QueryId { get; set; }
    public string? NewQueryName { get; set; }
    public string? NewQueryText { get; set; }
    public string? NewQueryCondition { get; set; }
    public int? Difficulty { get; set; }
    public int TimeLimit { get; set; }
    public bool IsNewQueryMode { get; set; }
}