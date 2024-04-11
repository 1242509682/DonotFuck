using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;


namespace DonotFuck
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        //作者名称
        public override string Author => "Cai 羽学修改";
        //插件的一句话描述
        public override string Description => "禁止脏话";
        //插件的名称
        public override string Name => "Don't Fuck";
        //插件的版本
        public override Version Version => new Version(2, 0, 0);

        internal static Configuration Config; //将Config初始化
        public static bool Enabled; // 存储插件是否Enabled的状态，默认为false
        string SaveDir = Path.Combine(TShock.SavePath, "禁止脏话");

        //插件的构造器
        public Plugin(Main game) : base(game)
        {
            Config = new Configuration();
            // 确保Config和DirtyWords都已经初始化
            if (Config == null || Config.DirtyWords == null)
                throw new InvalidOperationException("\n配置未正确初始化。");
        }


        //插件加载时执行的代码
        public override void Initialize()
        {
            LoadConfig();
            ServerApi.Hooks.ServerChat.Register(this, OnChat); //注册聊天钩子
            GeneralHooks.ReloadEvent += LoadConfig;
        }

        //重载配置文件
        private static void LoadConfig(ReloadEventArgs args = null)
        {
            Config = Configuration.Read(Configuration.FilePath);
            Config.Write(Configuration.FilePath);
            if (args != null && args.Player != null)
            {
                args.Player.SendSuccessMessage("[禁止脏话]重新加载配置完毕。");
            }
        }

        // 检查玩家聊天行为
        private void OnChat(ServerChatEventArgs args)
        {
            TSPlayer player = TShock.Players[args.Who];

            if (player == null || args.Who == null || player.HasPermission("Civilized") || player.Group.Name.Equals("owner", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            int WordsCount = 0;

            // 遍历脏话列表，计算本次聊天触发的脏话数量
            foreach (var badWord in Config.DirtyWords)
            {
                if (args.Text.Contains(badWord, StringComparison.OrdinalIgnoreCase))
                {
                    WordsCount++;
                }
            }

            // 如果触发了脏话，提醒玩家并更新累计违规次数
            if (WordsCount > 0)
            {
                string Text = args.Text; // 原始发言内容
                List<string> BadWordList = new List<string>(); // 存储玩家准确的脏话词语

                // 遍历脏话表检查是否有匹配项
                foreach (var badWord in Config.DirtyWords)
                {
                    if (Text.Contains(badWord, StringComparison.OrdinalIgnoreCase))
                    {
                        BadWordList.AddRange(GetExactMatches(Text, badWord)); // 获取并添加精确匹配的脏话
                    }
                }

                // 如果有触发脏话，显示给玩家的信息
                if (BadWordList.Any())
                {
                    string ShowBadWords = "";
                    foreach (string badWord in BadWordList)
                    {
                        ShowBadWords += $"- {badWord}\n";
                    }
                    TSPlayer.All.SendInfoMessage($"玩家[c/FFCCFF:{player.Name}]触发了以下敏感词：\n{ShowBadWords.TrimEnd('\n')}");

                    // 输出准确的脏话词语到控制台
                    foreach (string badWord in BadWordList)
                    {
                        TShock.Log.ConsoleInfo($"玩家 [{player.Name}] 发言中的脏话：{badWord}");
                    }
                }

                var Count = Ban.Trigger(player.Name);

                // 只有累计违规次数达到上限才发送提醒信息并执行封禁逻辑
                if (Count > Config.InspectedQuantity)
                {
                    TSPlayer.All.SendSuccessMessage($"玩家[c/FFCCFF:{player.Name}]被检测到多次用词不当！");
                    Console.WriteLine($"玩家[{player.Name}]被检测到多次用词不当！");

                    // 达到违规次数上限后执行封禁逻辑
                    if (Config.Ban)
                    {
                        Ban.AddBan(player.Name, $"不许说脏话", Config.ProhibitionTime * 60);
                        TSPlayer.All.SendInfoMessage($"{player.Name}已被封禁！原因：连续说了脏话");
                        TShock.Log.ConsoleInfo($"{player.Name}已被封禁！原因：连续说了脏话");//写入日志log
                        player.Disconnect($"你已被封禁！原因：连续说了脏话！");
                        Ban.Remove(player.Name); // 清零玩家违规次数
                        return;
                    }
                }
            }
        }

        // 定义获取原始文本中精确匹配脏话的辅助函数
        private static IEnumerable<string> GetExactMatches(string text, string badWord)
        {
            int index = 0;
            while ((index = text.IndexOf(badWord, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                yield return text.Substring(index, badWord.Length);
                index += badWord.Length;
            }
        }

        //释放钩子
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat); //卸载聊天钩子
            }
            base.Dispose(disposing);
        }
    }
}