using System.Text.Json;
using System.Text.Json.Nodes;
using CakeToolset.Attribute;
using CakeToolset.Global;
using CakeToolset.Global.Component;
using CakeToolset.Register;
using CommonUtil;
using CommonUtil.Extensions;
using Fractural.Tasks;
using Godot;
using FileAccess = Godot.FileAccess;

namespace ArcaneWorld.Global;

[Log]
[Tool]
public partial class ConfigManage : Node, IGlobalComponent {

    private static string filePath = "user://config.json";

    public static ConfigManage instance { get; private set; } = null!;

    private HashSet<Config> dirtySet = new HashSet<Config>();

    private JsonObject? cacheJObject;

    public void initialize() {

        instance = this;

        LoadConfigFromFile();

        GDTask.Create(
            async () => {
                CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();
                while (!cancellationToken.IsCancellationRequested) {
                    await Task.Delay(1000, cancellationToken);
                    if (dirtySet.Count == 0) {
                        continue;
                    }

                    cacheJObject ??= new JsonObject();

                    dirtySet
                        .TrySelect(
                            config => (config, data: config.saveToJson(JsonSerializerHold.jsonSerializerOptions)),
                            (config, e) => log.Error($"配置条目：{config.name} 保存Json数据时失败")
                        )
                        .Peek(t => cacheJObject[t.config.name] = t.data)
                        .End();
                }
            }
        );

    }

    public void terminate() {
        dirtySet.Clear();
        cacheJObject = null;

        instance = null!;
    }

    public void setDirty(Config config) {
        dirtySet.Add(config);
    }

    /// <summary>
    /// 重新加载配置文件
    /// </summary>
    public void ReloadConfig() {
        log.Info("开始重新加载配置文件");
        LoadConfigFromFile();
        log.Info("配置文件重新加载完成");
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    private void LoadConfigFromFile() {
        if (!FileAccess.FileExists(filePath)) {
            log.Warn("配置文件不存在，使用默认配置");
            return;
        }

        using FileAccess? file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

        if (file is null) {
            log.Error("配置文件无法正常读取，使用默认配置");
            return;
        }

        string asText = file.GetAsText();

        try {
            JsonObject? jObject = JsonSerializer.Deserialize<JsonObject>(asText, JsonSerializerHold.jsonSerializerOptions);

            if (jObject is null) {
                throw new Exception("配置文件无法序列化成Json对象");
            }

            cacheJObject = jObject;

            R_ConfigManage.instance.values
                .Select(config => (config, data: jObject[config.name]))
                .Where(
                    t => t.data is null,
                    t => log.Warn($"配置条目：{t.config.name} 配置数据丢失，将使用默认配置。")
                )
                .TryPeek(
                    t => t.config.setConfigFromJson(t.data!, JsonSerializerHold.jsonSerializerOptions),
                    (t, e) => log.Error($"配置条目：{t.config.name}，设置属性发生异常：", e)
                )
                .End();

        }
        catch (Exception e) {
            log.Error($"处理配置文件失败，原始内容：\n{asText}\n异常：", e);
        }
    }

    public int priority => 1 << 22;

}
