using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using ArcaneWorld.Global;
using Godot;

namespace ArcaneWorld;

internal class AppModule {

#pragma warning disable CA2255
    [System.Runtime.CompilerServices.ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize() {

        AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!.Unloading += c => {
            var assembly = typeof(JsonSerializerOptions).Assembly;
            var updateHandlerType = assembly.GetType("System.Text.Json.JsonSerializerOptionsUpdateHandler");
            var clearCacheMethod = updateHandlerType?.GetMethod("ClearCache", BindingFlags.Static | BindingFlags.Public);
            clearCacheMethod?.Invoke(null, [null]);
        };
    }

}
