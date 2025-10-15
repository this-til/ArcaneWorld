using Godot;

namespace ArcaneWorld.Util;

public partial class SimpleNode<S> : Node where S : SimpleNode<S> {

    public static S instance { get; private set; } = null!;

    public override void _Ready() {
        base._Ready();
        instance = (S)this;
    }

}
