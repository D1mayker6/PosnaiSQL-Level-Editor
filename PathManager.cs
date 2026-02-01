using System.IO;
using System.Text.Json;

namespace PosnaiSQLauncher;

public class PathManager
{
    public static string GetRootPath(PathMode mode)
    {
        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "databin.json");
        var json = File.ReadAllText(jsonPath);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        switch (mode)
        {
            case PathMode.Database:
                return root.GetProperty("DatabaseImagesPath").GetString();
            case PathMode.Option:
                return root.GetProperty("OptionsPath").GetString();
            case PathMode.Connection:
                return root.GetProperty("SqlConnection").GetString();
            case PathMode.Login:
                return root.GetProperty("DataStream").GetString();
            case PathMode.Password:
                return root.GetProperty("InfoVault").GetString();
            default:
                return string.Empty;
                break;
                
        }
    }

}