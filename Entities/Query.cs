using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosnaiSQLauncher.Entities;

public partial class Query
{
    public int IdQuery { get; set; }

    public string? Name { get; set; }

    [NotMapped]
    public string DecryptedName => CryptoManager.SafeDecrypt(Name);

    public string? Condition { get; set; }
    
    [NotMapped]
    public string DecryptedCondition => CryptoManager.SafeDecrypt(Condition);

    public string? QueryString { get; set; }
    
    [NotMapped]
    public string DecryptedQueryString => CryptoManager.SafeDecrypt(QueryString);

    public int Difficulty { get; set; }

    public int? IdDatabase { get; set; }

    public virtual Database? IdDatabaseNavigation { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
