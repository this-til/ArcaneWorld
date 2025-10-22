using System.Reflection;
using CakeToolset.Log;
using CommonUtil.Extensions;
using Godot;

namespace CakeToolset.Global.Component;

[Tool]
public partial class RegisterSystemHold : Node, IGlobalComponent {

    public static RegisterSystem.RegisterSystem registerSystem { get; private set; } = null!;

    public void initialize() {
        registerSystem = new RegisterSystem.RegisterSystem() {
            managedAssemblySet = GlobalComponentLoader.instance.loadAssembly.ToHashSet(),
            log = LogManager.GetLogger("RegisterSystem")
        };

        registerSystem.initRegisterSystem();
    }

    public void terminate() {
        registerSystem?.Dispose();
        registerSystem = null!;
    }

    public int priority => 1 << 24;

}
