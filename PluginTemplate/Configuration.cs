using Newtonsoft.Json;
using TShockAPI;

namespace DonotFuck
{
    public class Configuration
    {
        [JsonProperty("启用")]
        public bool Enabled { get; set; } = true;

        [JsonProperty("封禁时长")]
        public int ProhibitionTime = 10; // 分钟

        [JsonProperty("是否封禁")]
        public bool Ban = true; // 分钟

        [JsonProperty("检查次数")]
        public int InspectedQuantity = 5;  // 次

        [JsonProperty("脏话表")]
        public HashSet<string> DirtyWords { get; set; } = new HashSet<string>();

        public static readonly string FilePath = Path.Combine(TShock.SavePath, "禁止脏话.json");
        // 添加构造函数
        public Configuration()
        {
            DirtyWords = new HashSet<string>
            {
                "操",
                "妈的",
                "傻逼",
                "煞笔",
                "你妈",
                "草你",
            };
        }

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var str = JsonConvert.SerializeObject(this, Formatting.Indented);
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(str);
                }
            }
        }

        public static Configuration Read(string path)
        {
            if (!File.Exists(path))
            {
                var defaultConfig = new Configuration();
                defaultConfig.Write(path);
                return defaultConfig;
            }
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sr = new StreamReader(fs))
            {
                var cf = JsonConvert.DeserializeObject<Configuration>(sr.ReadToEnd());
                return cf;
            }
        }
    }
}