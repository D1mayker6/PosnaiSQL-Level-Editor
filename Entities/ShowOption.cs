using System;
using System.Collections.Generic;

namespace PosnaiSQLauncher.Entities;

public partial class ShowOption
{
    public int IdOption { get; set; }

    public string NameLocation { get; set; } = null!;

    public string NameDatabase { get; set; } = null!;

    public string? NameQuery { get; set; }

    public string? Condition { get; set; }

    public string? QueryString { get; set; }

    public int TimeLimit { get; set; }
}
