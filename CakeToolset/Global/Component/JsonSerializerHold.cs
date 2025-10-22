using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CakeToolset.Attribute;
using CommonUtil.Extensions;
using Godot;

namespace CakeToolset.Global.Component;

[Log]
[Tool]
public partial class JsonSerializerHold : Node, IGlobalComponent {

    public static JsonSerializerOptions jsonSerializerOptions { get; private set; } = null!;

    public void initialize() {

        jsonSerializerOptions = new JsonSerializerOptions {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        GlobalComponentLoader.instance.jsonConverterAutomaticLoadAttributeTypes
            .OrderByDescending(a => a.attribute.priority)
            .TrySelect(a => (JsonConverter)Activator.CreateInstance(a.type)!, (a, e) => log.Error($"从 {a.type} 创建 JsonConverter 失败:", e))
            .Peek(c => jsonSerializerOptions.Converters.Add(c))
            .End();

    }

    public void terminate() {
        jsonSerializerOptions = null!;

        Assembly assembly = typeof(JsonSerializerOptions).Assembly;
        Type? updateHandlerType = assembly.GetType("System.Text.Json.JsonSerializerOptionsUpdateHandler");
        MethodInfo? clearCacheMethod = updateHandlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
        clearCacheMethod?.Invoke(null, [null]);
    }

    public int priority => 1 << 23;

}
