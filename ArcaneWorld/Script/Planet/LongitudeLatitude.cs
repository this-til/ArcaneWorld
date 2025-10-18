using ArcaneWorld.Generated;
using ArcaneWorld.Generated.ShaderWrappers;
using CakeToolset.Log;
using CommonUtil.Log;
using Godot;

namespace ArcaneWorld.Planet;

[Tool]
[ClassName]
public partial class LongitudeLatitude : Node3D {

    private MeshInstance3D? meshIns;

    [ExportToolButton("手动触发重绘经纬线", Icon = "WorldEnvironment")]
    public Callable redraw => Callable.From(draw);

    [Export]
    public float radius { get; set; } = 110;

    [Export]
    public ShaderMaterial lineMaterial { get; set; } = new ShaderMaterial() { Shader = GD.Load<Shader>(R.Shaders.Planet.LongitudeLatitude.LineAlpha_gdshader) };

    public LineAlphaShader lineAlphaShader { get => field ??= new LineAlphaShader(lineMaterial); } = null!;

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

    [ExportGroup("透明度设置")]
    [Export(PropertyHint.Range, "0, 1")]
    public float visibility {
        get;
        set {
            field = Math.Clamp(value, 0, 1);
            lineAlphaShader.alphaFactor = field;
        }
    } = 0.5f;

    public override void _Ready() {
        meshIns = new MeshInstance3D();
        AddChild(meshIns);
        draw();
    }
    

    public void draw() {
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Lines);
        for (var i = -180 + longitudeInterval; i <= 180; i += longitudeInterval) {
            var longitudeRadian = Mathf.DegToRad(i);
            Color color = i is 0 or 180
                ? meridianColor
                : i is -90 or 90
                    ? degree90LongitudeColor
                    : i % (latitudeInterval * deeperLineInterval) == 0
                        ? deeperLineColor
                        : normalLineColor;
            drawLongitude(surfaceTool, longitudeRadian, color);
        }

        for (var i = -90 + latitudeInterval; i < 90; i += latitudeInterval) {
            Color color = i == 0
                ? equatorColor
                : i % (latitudeInterval * deeperLineInterval) == 0
                    ? deeperLineColor
                    : normalLineColor;
            var latitudeRadian = Mathf.DegToRad(i);
            drawLatitude(surfaceTool, latitudeRadian, color);
        }

        // 北极圈：北纬 66°34′
        if (drawArcticCircle) {
            drawLatitude(surfaceTool, Mathf.DegToRad(66.567f), circleColor, true);
        }
        // 北回归线：北纬 23°26′
        if (drawTropicOfCancer) {
            drawLatitude(surfaceTool, Mathf.DegToRad(23.433f), tropicColor, true);
        }
        // 南回归线：南纬 23°26′
        if (drawTropicOfCapricorn) {
            drawLatitude(surfaceTool, Mathf.DegToRad(-23.433f), tropicColor, true);
        }
        // 南极圈：南纬 66°34′
        if (drawAntarcticCircle) {
            drawLatitude(surfaceTool, Mathf.DegToRad(-66.567f), circleColor, true);
        }

        surfaceTool.SetMaterial(lineMaterial);
        meshIns!.Mesh = surfaceTool.Commit();
    }

    /// <summary>
    /// 绘制指定经线
    /// </summary>
    /// <param name="surfaceTool"></param>
    /// <param name="longitudeRadian">经度转为弧度制后的表示，+ 代表西经，- 代表东经（顺时针方向）</param>
    /// <param name="color"></param>
    private void drawLongitude(SurfaceTool surfaceTool, float longitudeRadian, Color color) {
        Vector3 equatorDirection = new Vector3(Mathf.Cos(longitudeRadian), 0, Mathf.Sin(longitudeRadian));

        // 北面
        draw90Degrees(surfaceTool, color, equatorDirection, Vector3.Up);
        // 南面
        draw90Degrees(surfaceTool, color, equatorDirection, Vector3.Down);
    }

    /// <summary>
    /// 绘制指定纬线
    /// </summary>
    /// <param name="surfaceTool"></param>
    /// <param name="latitudeRadian">维度转为弧度制后的表示，+ 表示北纬，- 表示南纬（上方取正）</param>
    /// <param name="color"></param>
    /// <param name="dash">是否按虚线绘制</param>
    private void drawLatitude(SurfaceTool surfaceTool, float latitudeRadian, Color color, bool dash = false) {
        var cos = Mathf.Cos(latitudeRadian); // 对应相比赤道应该缩小的比例
        var sin = Mathf.Sin(latitudeRadian); // 对应固定的高度
        // 本初子午线
        Vector3 primeMeridianDirection = new Vector3(cos, 0, 0);
        // 西经 90 度
        Vector3 west90Direction = new Vector3(0, 0, cos);
        draw90Degrees(
            surfaceTool,
            color,
            primeMeridianDirection,
            west90Direction,
            Vector3.Up * sin,
            dash
        );
        // 对向子午线
        Vector3 antiMeridianDirection = new Vector3(-cos, 0, 0);
        draw90Degrees(
            surfaceTool,
            color,
            west90Direction,
            antiMeridianDirection,
            Vector3.Up * sin,
            dash
        );
        // 东经 90 度
        Vector3 east90Direction = new Vector3(0, 0, -cos);
        draw90Degrees(
            surfaceTool,
            color,
            antiMeridianDirection,
            east90Direction,
            Vector3.Up * sin,
            dash
        );
        draw90Degrees(
            surfaceTool,
            color,
            east90Direction,
            primeMeridianDirection,
            Vector3.Up * sin,
            dash
        );
    }

    private void draw90Degrees
    (
        SurfaceTool surfaceTool,
        Color color,
        Vector3 from,
        Vector3 to,
        Vector3 origin = default,
        bool dash = false
    ) {
        Vector3 preDirection = from;
        for (var i = 1; i <= segments; i++) {
            Vector3 currentDirection = from.Slerp(to, (float)i / segments);
            if (!dash || i % 2 == 0) {
                surfaceTool.SetColor(color);
                // 【切记】：Mesh.PrimitiveType.Lines 绘制方式时，必须自己指定法线！！！否则没颜色
                surfaceTool.SetNormal(origin + preDirection);
                surfaceTool.AddVertex((origin + preDirection) * radius);
                surfaceTool.SetColor(color);
                surfaceTool.SetNormal(origin + currentDirection);
                surfaceTool.AddVertex((origin + currentDirection) * radius);
            }

            preDirection = currentDirection;
        }
    }

}
