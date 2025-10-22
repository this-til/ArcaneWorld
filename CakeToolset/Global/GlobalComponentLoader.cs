using System.Reflection;
using CakeToolset.Attribute;
using CommonUtil.Extensions;
using Godot;
using FileAccess = Godot.FileAccess;

namespace CakeToolset.Global;

public interface GlobalComponentLoader  {

    public static GlobalComponentLoader instance => GlobalComponentLoaderHole.instance;

    public IReadOnlyList<Assembly> loadAssembly { get; }

    public IReadOnlyList<(EventBusSubscriberAttribute attribute, Type type)> eventBusSubscriberAttributeTypes { get; }

    public IReadOnlyList<(JsonConverterAutomaticLoadAttribute attribute, Type type )> jsonConverterAutomaticLoadAttributeTypes { get; }

    public IReadOnlyList<Type> componentType { get; }

    public IReadOnlyDictionary<Type, IGlobalComponent> componentMap { get; }

    public IReadOnlyList<IGlobalComponent> componentList { get; }

}

public class GlobalComponentLoaderHole {

    public static GlobalComponentLoader instance { get; set; } = null!;

}
