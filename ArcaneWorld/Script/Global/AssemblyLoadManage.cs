using ArcaneWorld.Util;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArcaneWorld.Attribute;
using CommonUtil.Extensions;
using log4net;
using Array = Godot.Collections.Array;
using FileAccess = Godot.FileAccess;

namespace ArcaneWorld.Global;

public partial class AssemblyLoadManage : SimpleNode<AssemblyLoadManage> {

    public IReadOnlyList<Assembly> loadAssembly { get; private set; } = null!;

    public IReadOnlyList<(EventBusSubscriberAttribute attribute, Type type)> eventBusSubscriberAttributeTypes { get; private set; } = null!;

    public IReadOnlyList<(JsonConverterAutomaticLoadAttribute attribute, Type type )> jsonConverterAutomaticLoadAttributeTypes { get; private set; } = null!;

    private ILog log = null!;

    public override void _Ready() {
        base._Ready();
        log = LogManager.GetLogger(GetType());

        const string configPath = "res://assembly_load.config.json";

        try {
            // 读取配置文件
            var file = FileAccess.Open(configPath, FileAccess.ModeFlags.Read);
            if (file == null) {
                log.Error($"Failed to open config file: {configPath}");
                return;
            }

            string jsonString = file.GetAsText();
            file.Close();

            // 解析JSON数组
            Json json = new Json();
            Error parseResult = json.Parse(jsonString);

            if (parseResult != Error.Ok) {
                log.Error($"Failed to parse JSON config: {jsonString}");
                return;
            }

            loadAssembly =
                json.Data.AsGodotArray()
                    .Where(v => v.VariantType == Variant.Type.String)
                    .Select(v => v.AsString())
                    .ToList()
                    .TrySelect(
                        assemblyName => Assembly.Load(assemblyName),
                        (assemblyName, exception) => log.Error($"Failed to load assembly {assemblyName}:", exception)
                    )
                    .ToList()
                    .AsReadOnly();

        }
        catch (Exception ex) {
            log.Error($"Error loading assemblies from config: {ex.Message}");
            loadAssembly = new List<Assembly>() { GetType().Assembly };
        }

        List<Type> list = loadAssembly.SelectMany(a => a.GetTypes()).ToList();

        eventBusSubscriberAttributeTypes = list
            .Select(t => (attribute: t.GetCustomAttribute<EventBusSubscriberAttribute>(), t))
            .Where(t => t.attribute is not null)
            .ToList()!;

        jsonConverterAutomaticLoadAttributeTypes = list
            .Select(t => (attribute: t.GetCustomAttribute<JsonConverterAutomaticLoadAttribute>(), t))
            .Where(t => t.attribute is not null)
            .ToList()!;
    }

}
