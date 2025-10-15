using System.Reflection;
using ArcaneWorld.Attribute;
using CommonUtil.Extensions;
using Godot;

namespace ArcaneWorld.Global;

public partial class RegisterSystemHold : Node {

    public static RegisterSystem.RegisterSystem registerSystem { get; private set; } = null!;

    public override void _Ready() {
        base._Ready();
        registerSystem = new RegisterSystem.RegisterSystem() {
            managedAssemblySet = AssemblyLoadManage.instance.loadAssembly.ToHashSet(),
            log = log4net.LogManager.GetLogger("RegisterSystem")
        };

        registerSystem.initRegisterSystem();
    }

}
