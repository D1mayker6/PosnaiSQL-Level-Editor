using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PosnaiSQLauncher.Entities;

public partial class ShowOption
{
    public int IdOption { get; set; }

    public string NameLocation { get; set; } = null!;

    public string NameDatabase { get; set; } = null!;

    public string? NameQuery { get; set; }
    
    [NotMapped]
    public string DecryptedName => CryptoManager.SafeDecrypt(NameQuery);

    public string? Condition { get; set; }
    
    [NotMapped]
    public string DecryptedCondition => CryptoManager.SafeDecrypt(Condition);

    public string? QueryString { get; set; }
    
    [NotMapped]
    public string DecryptedQueryString => CryptoManager.SafeDecrypt(QueryString);

    public int Difficulty { get; set; }

    public int TimeLimit { get; set; }
}
