using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class DustMaterialManage : RegisterManage<DustMaterial> {

    public override void setup() {
        base.setup();
    }

}

public partial class DustMaterial : MaterialComponent {

    public override void setup() {
        base.setup();

        // 粉
        dust = new Item();

        // 小堆粉
        smallPileDust = new Item();

        // 小撮粉
        tinyPileDust = new Item();

    }

}
