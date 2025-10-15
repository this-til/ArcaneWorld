using ArcaneWorld.Util;
using Godot;
using System.IO;
using System.Text;
using log4net;
using log4net.Config;
using log4net.Repository;
using Environment = System.Environment;
using FileAccess = Godot.FileAccess;

namespace ArcaneWorld.Global;

public partial class LogManage : Node {
    

    public override void _Ready() {
        

        try {
            // 使用 Godot API 读取 log4net.config 文件
            const string configPath = "res://log4net.config";

            if (!FileAccess.FileExists(configPath)) {
                GD.PrintErr($"找不到 log4net 配置文件: {configPath}");
                return;
            }

            // 使用 Godot FileAccess 读取配置文件内容
            using var file = FileAccess.Open(configPath, FileAccess.ModeFlags.Read);
            if (file == null) {
                GD.PrintErr($"无法打开 log4net 配置文件: {configPath}");
                return;
            }

            string configContent = file.GetAsText();
            if (string.IsNullOrEmpty(configContent)) {
                GD.PrintErr("log4net 配置文件内容为空");
                return;
            }

            // 设置日志文件路径属性
            string logDirectory = Path.Combine(OS.GetUserDataDir(), "logs");
            if (!Directory.Exists(logDirectory)) {
                Directory.CreateDirectory(logDirectory);
            }

            using var configStream = new MemoryStream(Encoding.UTF8.GetBytes(configContent));
            XmlConfigurator.Configure(configStream);

            log4net.LogManager.GetLogger("LogManage").Info($"log4net 配置加载成功，日志目录: {logDirectory}");
        }
        catch (Exception ex) {
            GD.PrintErr($"初始化 log4net 失败: {ex.Message}");
        }

    }

}
