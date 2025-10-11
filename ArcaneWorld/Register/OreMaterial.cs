using ArcaneWorld.Register;
using FlexibleRequired;
using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class OreMaterialManage : RegisterManage<OreMaterial> {

    public override int priority => (int)RegisterPriority.OreMaterial;

    public override void setup() {
        base.setup();
    }

}

public partial class OreMaterial : MaterialComponent {


    public override void setup() {
        base.setup();

        // 原矿石
        rawOre = new Item();

        // 粉碎矿
        crushedOre = new Item();

    }

}
