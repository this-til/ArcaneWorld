#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ArcaneWorld.addons.ArcaneWorldGenerator;

[Tool]
public partial class ArcaneWorldGenerator : EditorPlugin {

    public const string generateResourceTreeKey = "生成资源树";

    public const string generateShaderOperationPacksKey = "生成着色器操作包装";

    public override void _EnterTree() {
        AddToolMenuItem(generateResourceTreeKey, new Callable(this, nameof(generateResourceTree)));
        AddToolMenuItem(generateShaderOperationPacksKey, new Callable(this, nameof(generateShaderOperationPacks)));
    }

    public override void _ExitTree() {
        RemoveToolMenuItem(generateResourceTreeKey);
        RemoveToolMenuItem(generateShaderOperationPacksKey);
    }

    private void generateResourceTree() {
        GenerateResourceTree.generateResourceTree();
    }

    private void generateShaderOperationPacks() {
        GenerateShaderOperationPacks.generateShaderOperationPacks();
    }

}

#endif
