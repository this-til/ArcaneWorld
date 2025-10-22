using System.Reflection;
using CakeToolset.Attribute;
using CakeToolset.Global;
using CommonUtil.Extensions;
using Godot;
using FileAccess = Godot.FileAccess;

[Log]
[Tool]
public partial class S_GlobalComponentLoader : Node, GlobalComponentLoader, ISerializationListener {

    public IReadOnlyList<Assembly> loadAssembly { get; private set; } = null!;

    public IReadOnlyList<(EventBusSubscriberAttribute attribute, Type type)> eventBusSubscriberAttributeTypes { get; private set; } = null!;

    public IReadOnlyList<(JsonConverterAutomaticLoadAttribute attribute, Type type )> jsonConverterAutomaticLoadAttributeTypes { get; private set; } = null!;

    public IReadOnlyList<Type> componentType { get; private set; } = null!;

    public IReadOnlyDictionary<Type, IGlobalComponent> componentMap { get; private set; } = null!;

    public IReadOnlyList<IGlobalComponent> componentList { get; private set; } = null!;

    public bool initialized { get; private set; }

    public override void _Ready() {
        base._Ready();
        initialize();
    }

    void initialize() {
        GlobalComponentLoaderHole.instance = this;

        if (initialized) {
            return;
        }

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
                        Assembly.Load,
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

        componentType = list
            .Where(t => !t.IsAbstract)
            .Where(t => typeof(IGlobalComponent).IsAssignableFrom(t))
            .ToList();

        componentMap = componentType
            .TrySelect(t => (type: t, component: (IGlobalComponent)Activator.CreateInstance(t)!), (type, exception) => log.Error($"创建 {type} 的实例失败:", exception))
            .ToDictionary(t => t.type, t => t.component);

        componentList = componentMap.Values.OrderByDescending(c => c.priority).ToList();

        componentList
            .Peek(c => c.asNode.Name = c.GetType().Name)
            .Peek(c => AddChild(c.asNode))
            .TryPeek(
                c => c.initialize(),
                (component, exception) => log.Error($"初始化 {component.GetType()} 时出现错误:", exception)
            )
            .End();

    }

    void terminate() {
        initialized = false;

        GlobalComponentLoaderHole.instance = null!;
        
        loadAssembly = null!;
        eventBusSubscriberAttributeTypes = null!;
        jsonConverterAutomaticLoadAttributeTypes = null!;
        componentType = null!;
        componentMap = null!;

        componentList?
            .TryPeek(
                c => c.terminate(),
                (component, exception) => log.Error($"卸载 {component.GetType()} 时出现错误:", exception)
            )
            .Peek(c => c.asNode.Free())
            .End();
        
        componentList = null!;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        log.Debug("terminate end...........");

    }

    public void OnBeforeSerialize() {
        log.Info("OnBeforeSerialize...");
        terminate();
    }

    public void OnAfterDeserialize() {
        log.Info("OnAfterDeserialize...");
        initialize();
    }

}
