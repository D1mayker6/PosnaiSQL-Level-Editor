using System;
using System.Collections.Generic;

namespace PosnaiSQLauncher.Entities;

public partial class Query
{
    public int IdQuery { get; set; }

    public string? Name { get; set; }

    public string? Condition { get; set; }

    public string? QueryString { get; set; }

    public int? IdDatabase { get; set; }

    public virtual Database? IdDatabaseNavigation { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
