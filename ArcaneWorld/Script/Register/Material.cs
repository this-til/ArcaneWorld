using FlexibleRequired;
using RegisterSystem;

namespace ArcaneWorld.Register;

public partial class MaterialManage : RegisterManage<Material> {

    public override int priority => (int)RegisterPriority.Material;

    protected override void setup() {
        base.setup();

        // 铁
        iron = new Material() {
            createOreMaterial = m => new OreMaterial() {
                material = m
            },
            createMetalMaterial = m => new MetalMaterial() {
                material = m
            }
        };

    }

}

public partial class Material : RegisterBasics {

    public Func<Material, OreMaterial>? createOreMaterial { protected get; init; }

    public OreMaterial? oreMaterial { get; private set; }

    public Func<Material, GemstoneMaterial>? createGemstoneMaterial { protected get; init; }

    public GemstoneMaterial? gemstoneMaterial { get; private set; }

    public Func<Material, MetalMaterial>? createMetalMaterial { protected get; init; }

    public MetalMaterial? metalMaterial { get; private set; }

    protected override void setup() {
        base.setup();

        dustMaterial = new DustMaterial() {
            material = this
        };

        if (createOreMaterial != null) {
            oreMaterial = createOreMaterial(this);
            if (!Equals(oreMaterial.material, this)) {
                throw new Exception();
            }
        }

        if (createGemstoneMaterial != null) {
            gemstoneMaterial = createGemstoneMaterial(this);
            if (!Equals(gemstoneMaterial.material, this)) {
                throw new Exception();
            }
        }

        if (createMetalMaterial != null) {
            metalMaterial = createMetalMaterial(this);
            if (!Equals(metalMaterial.material, this)) {
                throw new Exception();
            }
        }
    }

}

public partial class MaterialComponent : RegisterBasics {

    [Required]
    public Material material { get; init; } = null!;

}
