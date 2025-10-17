using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class ItemManage : RegisterManage<Item> {

    public override int priority => (int)RegisterPriority.Item;

    protected override void setup() {
        base.setup();
    }

}

public partial class Item : RegisterBasics {

}
