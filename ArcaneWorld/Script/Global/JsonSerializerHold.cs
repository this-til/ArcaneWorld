using ArcaneWorld.Util;
using CommonUtil.Extensions;
using Godot;
using log4net;
using log4net.Core;
using log4net.Filter;
using Newtonsoft.Json;

namespace ArcaneWorld.Global;

public partial class JsonSerializerHold : Node {

    public static JsonSerializer jsonSerializer { get; private set; } = null!;

    private ILog log = null!;

    public override void _Ready() {
        base._Ready();

        log = LogManager.GetLogger(GetType());

        jsonSerializer = new JsonSerializer();
        jsonSerializer.Formatting = Formatting.Indented;
        
        AssemblyLoadManage.instance.jsonConverterAutomaticLoadAttributeTypes
            .OrderByDescending(a => a.attribute.priority)
            .TrySelect(a => (JsonConverter)Activator.CreateInstance(a.type)!, (a, e) => log.Error($"从 {a.type} 创建 JsonConverter 失败:", e))
            .Peek(c => jsonSerializer.Converters.Add(c))
            .End();

    }

}
