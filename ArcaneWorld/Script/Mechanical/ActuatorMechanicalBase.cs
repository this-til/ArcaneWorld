using ArcaneWorld.Attribute;
using ArcaneWorld.Capacity;
using ArcaneWorld.Capacity.Instance;
using ArcaneWorld.Register;

namespace ArcaneWorld.Mechanical;

public partial class ActuatorMechanicalBase : MechanicalBase, IItemHandler, IFluidHandler, IEnergyHandler {

    [SaveField]
    protected Container<Item> inputItemCache { get; }

    [SaveField]
    protected Container<Item> outputItemCache { get; }

    [SaveField]
    protected Container<Fluid> inputFluidCache { get; }

    [SaveField]
    protected Container<Fluid> outputFluidCache { get; }

    [SaveField]
    protected Container<OriginalVis> energyCache { get; }

    public ActuatorMechanicalBase() {
        inputItemCache = new Container<Item>() {
            transferLock = this
        };

        outputItemCache = new Container<Item>() {
            transferLock = this
        };

        inputFluidCache = new Container<Fluid>() {
            transferLock = this
        };

        outputFluidCache = new Container<Fluid>() {
            transferLock = this
        };

        energyCache = new Container<OriginalVis>() {
            transferLock = this
        };
    }

    public long insert(Item item, long count, bool simulation) {
        return inputItemCache.insert(item, count, simulation);
    }

    public long extract(Item item, long count, bool simulation) {
        return outputItemCache.extract(item, count, simulation);
    }

    public long insert(Fluid item, long count, bool simulation) {
        return inputFluidCache.insert(item, count, simulation);
    }

    public long extract(Fluid item, long count, bool simulation) {
        return outputFluidCache.extract(item, count, simulation);
    }

    public long insert(OriginalVis item, long count, bool simulation) {
        return energyCache.insert(item, count, simulation);
    }

    public long extract(OriginalVis item, long count, bool simulation) {
        return energyCache.extract(item, count, simulation);
    }

}

/*
public enum EnergyType {

    //发电机
    generator,

    //负载
    load,

    //储点器
    storagePointDevice

}
*/
