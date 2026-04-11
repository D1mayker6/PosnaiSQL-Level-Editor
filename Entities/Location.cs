using System;
using System.Collections.Generic;

namespace PosnaiSQLauncher.Entities;

public partial class Location
{
    public int IdLocation { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
