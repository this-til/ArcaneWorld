using ArcaneWorld.Planet;
using FlexibleRequired;
using Godot;
using RegisterSystem;

namespace ArcaneWorld.Register.Planet;

public partial class PlanetMeshTypeManage : RegisterManage<PlanetMeshType> {

    protected override void setup() {
        base.setup();

        terrain = new PlanetMeshType() {
            createMaterialFactory = _ => new Godot.Material()
        };
        water = new PlanetMeshType() {
            createMaterialFactory = _ => new Godot.Material()
        };
    }

}

public partial class PlanetMeshType : RegisterBasics {

    [Required]
    public Func<ChunkLod, Godot.Material> createMaterialFactory { get; init; } = null!;

    public virtual SurfaceTool createSurfaceTool(ChunkLod chunkLod) {
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        return surfaceTool;
    }

    public virtual Mesh toMesh(SurfaceTool surfaceTool, ChunkLod chunkLod) {
        surfaceTool.GenerateNormals();
        ArrayMesh arrayMesh = surfaceTool.Commit();
        return arrayMesh;
    }

    public virtual MeshInstance3D createMeshInstance(ChunkLod chunkLod) => new MeshInstance3D() {
        Name = name.ToString()
    };

    public virtual Godot.Material createMaterial(ChunkLod chunkLod) => createMaterialFactory(chunkLod);

}
