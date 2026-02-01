using System;
using System.Collections.Generic;

namespace PosnaiSQLauncher.Entities;

public partial class Database
{
    public int IdDatabase { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Query> Queries { get; set; } = new List<Query>();
}
