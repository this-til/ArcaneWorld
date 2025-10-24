using Godot;

namespace ArcaneWorld.Util.Extensions;

public static class SurfaceToolExtensions {
    
    /// <summary>
    /// 添加三角形扇形，用于六边形地块的三角化
    /// </summary>
    public static void AddTriangleFan(this SurfaceTool surfaceTool, Vector3[] vertices, Color[]? colors = null) {
        if (vertices.Length < 3) return;
        
        // 扇形三角化：以第一个顶点为中心，连接相邻顶点形成三角形
        Vector3 center = vertices[0];
        Color centerColor = colors?[0] ?? Colors.White;
        
        for (int i = 1; i < vertices.Length - 1; i++) {
            // 添加三角形顶点（center, vertices[i], vertices[i+1]）
            surfaceTool.SetColor(centerColor);
            surfaceTool.AddVertex(center);
            
            surfaceTool.SetColor(colors?[i] ?? Colors.White);
            surfaceTool.AddVertex(vertices[i]);
            
            surfaceTool.SetColor(colors?[i + 1] ?? Colors.White);
            surfaceTool.AddVertex(vertices[i + 1]);
        }
    }
}
