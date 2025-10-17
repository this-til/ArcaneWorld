using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class FluidManage : RegisterManage<Fluid> {

    public override int priority => (int)RegisterPriority.Fluid;

    protected override void setup() {
        base.setup();
    }

}

public partial class Fluid : RegisterBasics {

}
