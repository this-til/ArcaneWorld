using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class DustMaterialManage : RegisterManage<DustMaterial> {

    protected override void setup() {
        base.setup();
    }

}

public partial class DustMaterial : MaterialComponent {

    protected override void setup() {
        base.setup();

        // 粉
        dust = new Item();

        // 小堆粉
        smallPileDust = new Item();

        // 小撮粉
        tinyPileDust = new Item();

    }

}
