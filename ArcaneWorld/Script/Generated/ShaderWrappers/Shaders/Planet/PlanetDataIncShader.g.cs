using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: PlanetData.gdshaderinc
/// 路径: res://Shaders/Planet/PlanetData.gdshaderinc
/// </summary>
public class PlanetDataIncShader {

    private readonly ShaderMaterial _material;

    public PlanetDataIncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: radius
    /// </summary>
    public float radius {
        get => _material.GetShaderParameter("radius").As<float>();
        set => _material.SetShaderParameter("radius", value);
    }

    /// <summary>
    /// Shader 参数: atmosphereHeight
    /// </summary>
    public float atmosphereHeight {
        get => _material.GetShaderParameter("atmosphereHeight").As<float>();
        set => _material.SetShaderParameter("atmosphereHeight", value);
    }

    /// <summary>
    /// Shader 参数: chunkDivisions
    /// </summary>
    public int chunkDivisions {
        get => _material.GetShaderParameter("chunkDivisions").As<int>();
        set => _material.SetShaderParameter("chunkDivisions", value);
    }

    /// <summary>
    /// Shader 参数: tileDivisions
    /// </summary>
    public int tileDivisions {
        get => _material.GetShaderParameter("tileDivisions").As<int>();
        set => _material.SetShaderParameter("tileDivisions", value);
    }

}
