using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class GemstoneMaterialManage : RegisterManage<GemstoneMaterial> {

    protected override void setup() {
        base.setup();
    }

}

public partial class GemstoneMaterial : MaterialComponent {

    protected override void setup() {
        base.setup();

        //完美 8粉
        perfect = new Item();

        //无暇 4粉
        flawless = new Item();
        
        //有瑕 2粉
        flaw = new Item();
        
        //破碎 1粉
        broken = new Item();

    }

}