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
        //定义插件的作者名称
        public override string Author => "Cai 羽学修改";
        //插件的一句话描述
        public override string Description => "拒绝脏话";
        //插件的名称
        public override string Name => "Don't Fuck";
        //插件的版本
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        internal static Configuration Config; //将Config初始化
        public static bool Enabled; // 存储插件是否Enabled的状态，默认为false
        string SaveDir = Path.Combine(TShock.SavePath, "禁止脏话");

        //插件的构造器
        public Plugin(Main game) : base(game)
        {
            Config = Configuration.Read(Configuration.FilePath);
            Enabled = Config.Enabled;
        }


        //插件加载时执行的代码
        public override void Initialize()
        {
            if (!Directory.Exists(SaveDir))
            {
                Directory.CreateDirectory(SaveDir);
            }
            ServerApi.Hooks.ServerChat.Register(this, OnChat); //注册聊天钩子
            GeneralHooks.ReloadEvent += ReloadConfig;
        }

        private static void LoadConfig()
        {
            Config = Configuration.Read(Configuration.FilePath);
            Enabled = Config.Enabled; // 更新全局Enabled状态
            Config.Write(Configuration.FilePath);
        }

        private static void ReloadConfig(ReloadEventArgs args = null)
        {
            LoadConfig();
            // 如果 args 不为空，则发送重载成功的消息
            if (args != null && args.Player != null)
            {
                args.Player.SendSuccessMessage("[禁止脏话]重新加载配置完毕。");
            }
        }


        private void OnChat(ServerChatEventArgs args)
        {
            TSPlayer plr = TShock.Players[args.Who];

            if (!plr.HasPermission("Civilized"))
            {
                if (plr != null && plr.Group.Name.Equals("owner", StringComparison.OrdinalIgnoreCase))
                {
                    return; // 如果是owner组，则跳过审查
                }

                int triggeredWordsCount = 0; //触发字数

                // 遍历脏话表检查是否有匹配项
                foreach (var badWord in Config.DirtyWords)
                {
                    bool shouldBan = false;
                    var name = plr.Name;
                    var num = Config.InspectedQuantity;
                    var max = Ban.Trigger(name);
                    if (args.Text.Contains(badWord, StringComparison.OrdinalIgnoreCase))
                    {
                        triggeredWordsCount++;
                        if (num >= max)
                        {
                            shouldBan = true;
                            break;
                        }
                        else
                        {
                            // plr.Kick("不许说脏脏！", true);
                            TSPlayer.All.SendSuccessMessage($"玩家[c/FFCCFF:{name}]被检测到用词不当！");
                            Console.WriteLine($"玩家[{name}]被检测到用词不当！");
                        }
                    }
                    if (Config.Ban)
                    {
                        Ban.Remove(name);
                        TSPlayer.All.SendInfoMessage($"{name}已被封禁！原因：说了脏话。");
                        plr.Disconnect($"你已被封禁！原因：说了脏话！。");
                        Ban.AddBan(name, $"不许说脏话", Config.ProhibitionTime * 60);
                        return; //跳出循环
                    }
                }
            }
        }

        //插件卸载时执行的代码
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