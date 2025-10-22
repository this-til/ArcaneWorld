using Godot;

namespace ArcaneWorld.Util;

public static class MeshConstant {

    public static readonly Color Weights1 = Colors.Red;
    public static readonly Color Weights2 = Colors.Green;
    public static readonly Color Weights3 = Colors.Blue;

    public static T[] TriArr<T>(T c) => [c, c, c];
    public static T[] QuadArr<T>(T c) => [c, c, c, c];
    public static T[] QuadArr<T>(T c1, T c2) => [c1, c1, c2, c2];

}
