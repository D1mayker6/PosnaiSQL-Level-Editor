using System;
using System.Collections.Generic;

namespace PosnaiSQLauncher.Entities;

public partial class Option
{
    public int IdOption { get; set; }

    public int? IdQuery { get; set; }

    public int TimeLimit { get; set; }

    public int? IdLocation { get; set; }

    public virtual Location? IdLocationNavigation { get; set; }

    public virtual Query IdQueryNavigation { get; set; } = null!;
}
