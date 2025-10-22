using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Nodes;
using ArcaneWorld.Global;
using RegisterSystem;
using Godot;
using CakeToolset.Log;
using CakeToolset.Util.Extensions;
using CommonUtil;
using CommonUtil.Log;
using EventBus;
using FileAccess = Godot.FileAccess;

namespace CakeToolset.Register;

public partial class R_ConfigManage : RegisterManage<Config> {

    protected override void setup() {
        base.setup();
    }

}

public abstract partial class Config : RegisterBasics {

    public abstract Type dataType { get; }

    public abstract void setConfigFromJson(JsonNode? node, JsonSerializerOptions options);
    public abstract JsonNode? saveToJson(JsonSerializerOptions options);

}

/// <summary>
/// 配置类
/// 并不欢迎使用 T? 
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class Config<T> : Config {

    public override Type dataType => typeof(T);

    private T _value = default!;

    public T value {
        get => _value ??= defaultDate();
        set {
            if (Equals(_value, value)) {
                return;
            }
            _value = value;

            releaseUpdate();
            ConfigManage.instance.setDirty(this);
        }
    }

    [Required]
    public Func<T> defaultDate { protected get; init; } = null!;

    public override void setConfigFromJson(JsonNode? node, JsonSerializerOptions options) {
        if (node == null) {
            value = defaultDate();
            return;
        }
        value = JsonSerializer.Deserialize<T>(node, options) ?? defaultDate();
    }

    public override JsonNode? saveToJson(JsonSerializerOptions options) {
        return JsonSerializer.SerializeToNode(value, options);
    }

    public virtual void releaseUpdate() {
        new Event.ConfigEvent.ConfigChangeEvent {
            overallConfig = this
        }.onEvent();
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    private string GetConfigFilePath() {
        string configDirectory = Path.Combine(OS.GetUserDataDir(), "config");
        return Path.Combine(configDirectory, $"{name}.json");
    }

}
