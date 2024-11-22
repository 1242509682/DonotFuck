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
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(FilePath));
        }
        else
        {
            var jsonContent = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
        }
    }

    public static void WriteExample()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        var NameSpace = assembly.GetName().Name.Trim();
        var FolderName = "内嵌文件".Trim();
        var FullName = $"{NameSpace}.{FolderName}.禁止脏话.json";

        Stream ResourceStream = assembly.GetManifestResourceStream(FullName);

        if (ResourceStream == null)
        {
            throw new InvalidOperationException($"无法找到嵌入资源：{FullName}");
        }

        using (StreamReader streamReader = new StreamReader(ResourceStream))
        {
            string text = streamReader.ReadToEnd();
            Configuration config = JsonConvert.DeserializeObject<Configuration>(text);
            config.Write();
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