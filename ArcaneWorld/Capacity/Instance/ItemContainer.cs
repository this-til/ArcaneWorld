using ArcaneWorld.Register;

namespace ArcaneWorld.Capacity.Instance;

[ClassName]
public partial class ItemContainer : Container<Item> {

    protected override string convertToGodotKey(Item item) {
        return item.name;
    }

    protected override Item convertFromGodotKey(string key) {
        return null!;
    }

}
