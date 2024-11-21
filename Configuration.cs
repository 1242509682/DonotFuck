using System.Reflection;
using Newtonsoft.Json;
using TShockAPI;

namespace DonotFuck;

public class Configuration
{
    [JsonProperty("脏话表")]
    public HashSet<string> DirtyWords { get; set; } = new HashSet<string>();

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
}