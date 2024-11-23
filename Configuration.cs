using System.Reflection;
using Newtonsoft.Json;
using TShockAPI;

namespace DonotFuck;

public class Configuration
{
    #region 实例变量
    [JsonProperty("每页行数")]
    public int PageSize = 30;

    [JsonProperty("记录日志")]
    public bool Log = true;

    [JsonProperty("脏话表")]
    public HashSet<string> DirtyWords { get; set; } = new HashSet<string>();
    #endregion

    #region 读取与创建配置文件方法
    public static readonly string FilePath = Path.Combine(TShock.SavePath, "禁止脏话", "禁止脏话.json");

    public void Write()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    public static Configuration Read()
    {
        if (!File.Exists(FilePath))
        {
            WriteExample();
        }

        return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(FilePath))!;
    }

    public static void WriteExample() //内嵌文件方法
    {
        var assembly = Assembly.GetExecutingAssembly();
        var FullName = $"{assembly.GetName().Name}.内嵌文件.禁止脏话.json";

        using (var stream = assembly.GetManifestResourceStream(FullName))
        using (var reader = new StreamReader(stream!))
        {
            var text = reader.ReadToEnd();
            var config = JsonConvert.DeserializeObject<Configuration>(text);
            config!.Write();
        }
    }
    #endregion

    #region 增删改方法
    internal bool Exempt(string text)
    {
        return this.DirtyWords.Contains(text);
    }

    public bool Add(string text)
    {
        if (this.Exempt(text))
        {
            return false;
        }
        this.DirtyWords.Add(text);
        this.Write();
        return true;
    }

    public bool Del(string text)
    {
        if (this.Exempt(text))
        {
            this.DirtyWords.Remove(text);
            this.Write();
            return true;
        }
        return false;
    }
    #endregion
}