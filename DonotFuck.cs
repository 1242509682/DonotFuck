using System.Text.RegularExpressions;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace DonotFuck;

[ApiVersion(2, 1)]
public class Plugin : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "Don't Fuck";
    public override string Author => "Cai 羽学";
    public override string Description => "当玩家聊天有敏感词时用*号代替该词";
    public override Version Version => new Version(3, 0, 0);
    #endregion

    #region 实例变量
    internal static Configuration Config;
    string FilePath = Path.Combine(TShock.SavePath, "禁止脏话");
    #endregion

    #region 注册与释放
    public Plugin(Main game) : base(game){}

    //注册
    public override void Initialize()
    {
        // 检查配置文件夹是否存在，不存在则根据FilePath路径创建。
        if (!Directory.Exists(FilePath))
        {
            Directory.CreateDirectory(FilePath); // 自动创建缺失的文件夹。
        }

        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        ServerApi.Hooks.ServerChat.Register(this, OnChat);
    }

    //释放
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置重载读取与写入方法
    private static void ReloadConfig(ReloadEventArgs args = null!)
    {
        LoadConfig();
        args.Player.SendInfoMessage("[禁止脏话]重新加载配置完毕。");
    }
    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Config.Write();
    }
    #endregion

    #region 检查玩家聊天行为
    private void OnChat(ServerChatEventArgs args)
    {
        TSPlayer plr = TShock.Players[args.Who];

        if (plr == null || plr.HasPermission("DonotFuck") || plr.Group.Name.Equals("owner", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var Text = args.Text;
        var Count = 0;

        foreach (var bad in Config.DirtyWords)
        {
            if (Text.Contains(bad, StringComparison.OrdinalIgnoreCase))
            {
                Count++;
                var Replace = new string('*', bad.Length); // 创建与脏话等长的星号字符串
                Text = Regex.Replace(Text, bad, Replace, RegexOptions.IgnoreCase); // 替换脏话
            }
        }

        if (Count > 0)
        {
            TSPlayer.All.SendMessage(string.Format(TShock.Config.Settings.ChatFormat, plr.Group.Name, plr.Group.Prefix, plr.Name, plr.Group.Suffix, Text), plr.Group.R, plr.Group.G, plr.Group.B);
            Console.Write(string.Format($"〖{plr.Group.Name}〗[{args.Who}] {plr.Name}：{args.Text}\n"));
            args.Handled = true;
        }
    }
    #endregion

}