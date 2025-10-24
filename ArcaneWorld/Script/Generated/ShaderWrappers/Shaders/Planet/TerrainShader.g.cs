using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: Terrain.gdshader
/// 路径: res://Shaders/Planet/Terrain.gdshader
/// </summary>
public class TerrainShader {

    private readonly ShaderMaterial _material;

    public TerrainShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: color
    /// </summary>
    public Color color {
        get => _material.GetShaderParameter("color").As<Color>();
        set => _material.SetShaderParameter("color", value);
    }

    /// <summary>
    /// Shader 参数: main_tex
    /// </summary>
    public Color mainTex {
        get => _material.GetShaderParameter("main_tex").As<Color>();
        set => _material.SetShaderParameter("main_tex", value);
    }

    /// <summary>
    /// Shader 参数: glossiness
    /// </summary>
    public float glossiness {
        get => _material.GetShaderParameter("glossiness").As<float>();
        set => _material.SetShaderParameter("glossiness", value);
    }

    /// <summary>
    /// Shader 参数: specular
    /// </summary>
    public float specular {
        get => _material.GetShaderParameter("specular").As<float>();
        set => _material.SetShaderParameter("specular", value);
    }

    /// <summary>
    /// Shader 参数: background_color
    /// </summary>
    public Color backgroundColor {
        get => _material.GetShaderParameter("background_color").As<Color>();
        set => _material.SetShaderParameter("background_color", value);
    }

    /// <summary>
    /// Global Shader 参数: radius
    /// </summary>
    public float radius {
        get => RenderingServer.GlobalShaderParameterGet("radius").As<float>();
        set => RenderingServer.GlobalShaderParameterSet("radius", value);
    }

    /// <summary>
    /// Global Shader 参数: max_height
    /// </summary>
    public float maxHeight {
        get => RenderingServer.GlobalShaderParameterGet("max_height").As<float>();
        set => RenderingServer.GlobalShaderParameterSet("max_height", value);
    }

    /// <summary>
    /// Global Shader 参数: inv_planet_matrix
    /// </summary>
    public Transform3D invPlanetMatrix {
        get => RenderingServer.GlobalShaderParameterGet("inv_planet_matrix").As<Transform3D>();
        set => RenderingServer.GlobalShaderParameterSet("inv_planet_matrix", value);
    }

}
