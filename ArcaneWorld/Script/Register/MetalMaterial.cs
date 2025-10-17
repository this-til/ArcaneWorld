using ArcaneWorld.Register;
using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class MetalMaterialManage : RegisterManage<MetalMaterial> {

    protected override void setup() {
        base.setup();
    }

}

public partial class MetalMaterial : MaterialComponent {

    protected override void setup() {
        base.setup();

        // 锭
        ingot = new Item();

        // 粒
        nugget = new Item();

        // 齿轮
        gear = new Item();

        // 板
        plate = new Item();

        // 外壳
        shell = new Item();

        // 箔
        foil = new Item();

        // 杆
        rod = new Item();

        // 丝
        wire = new Item();

        // 环
        ring = new Item();

        // 框架
        framework = new Item();

    }

}
