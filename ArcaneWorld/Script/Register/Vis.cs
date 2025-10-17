using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class VisManage : RegisterManage<Vis> {

    protected override void setup() {
        base.setup();
    }
}

public partial class Vis : RegisterBasics {

  

}

public partial class OriginalVis : Vis {

}

public partial class AdvancedVis : Vis {

}
