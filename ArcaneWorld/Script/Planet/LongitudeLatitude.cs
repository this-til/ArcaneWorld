using ArcaneWorld.Generated;
using CakeToolset.Log;
using CommonUtil.Log;
using Godot;

namespace ArcaneWorld.Planet;

[Tool]
[ClassName]
public partial class LongitudeLatitude : Node3D {

    private MeshInstance3D? meshIns;

    [Export]
    public ShaderMaterial lineMaterial { get; set; } = new ShaderMaterial() { Shader = GD.Load<Shader>(R.Shaders.Planet.LongitudeLatitude.LineAlpha_gdshader) };

    [Export(PropertyHint.Range, "0, 180")]
    public int longitudeInterval { get; set; } = 15;

    [Export(PropertyHint.Range, "0, 90")]
    public int latitudeInterval { get; set; } = 15;

    [Export(PropertyHint.Range, "1, 100")]
    public int segments { get; set; } = 30; // 每个 90 度的弧线被划分多少段

    [ExportGroup("颜色设置")]
    [Export]
    public Color normalLineColor { get; set; } = Colors.SkyBlue;

    [Export]
    public Color deeperLineColor { get; set; } = Colors.DeepSkyBlue;

    [Export]
    public int deeperLineInterval { get; set; } = 3; // 更深颜色的线多少条出现一次

    [Export]
    public Color tropicColor { get; set; } = Colors.Green; // 南北回归线颜色

    [Export]
    public Color circleColor { get; set; } = Colors.Aqua; // 南北极圈颜色

    [Export]
    public Color equatorColor { get; set; } = Colors.Yellow; // 赤道颜色

    [Export]
    public Color degree90LongitudeColor { get; set; } = Colors.Orange; // 东西经 90 度颜色

    [Export]
    public Color meridianColor { get; set; } = Colors.Red; // 子午线颜色

    [ExportGroup("开关特定线显示")]
    [Export]
    public bool drawTropicOfCancer { get; set; } = true; // 是否绘制北回归线

    [Export]
    public bool drawTropicOfCapricorn { get; set; } = true; // 是否绘制南回归线

    [Export]
    public bool drawArcticCircle { get; set; } = true; // 是否绘制北极圈

    [Export]
    public bool drawAntarcticCircle { get; set; } = true; // 是否绘制南极圈

    /*[Export(PropertyHint.Range, "0, 1")]
    public float visibility {
        get => field;
        set {
            
        }
    } = 0.5f;*/

    private bool _fixFullVisibility;

    public override void _Ready() {
        meshIns = new MeshInstance3D();
        AddChild(meshIns);
    }

    public override void _Process(double delta) {
        base._Process(delta);
    }

}
