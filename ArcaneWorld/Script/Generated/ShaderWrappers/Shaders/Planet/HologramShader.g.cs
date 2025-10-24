using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: Hologram.gdshader
/// 路径: res://Shaders/Planet/Hologram.gdshader
/// </summary>
public class HologramShader {

    private readonly ShaderMaterial _material;

    public HologramShader(ShaderMaterial material) {
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
    /// Shader 参数: lines1
    /// </summary>
    public float lines1 {
        get => _material.GetShaderParameter("lines1").As<float>();
        set => _material.SetShaderParameter("lines1", value);
    }

    /// <summary>
    /// Shader 参数: lines2
    /// </summary>
    public float lines2 {
        get => _material.GetShaderParameter("lines2").As<float>();
        set => _material.SetShaderParameter("lines2", value);
    }

    /// <summary>
    /// Shader 参数: distortion_strength
    /// </summary>
    public float distortionStrength {
        get => _material.GetShaderParameter("distortion_strength").As<float>();
        set => _material.SetShaderParameter("distortion_strength", value);
    }

    /// <summary>
    /// Shader 参数: distortion_percentage
    /// </summary>
    public float distortionPercentage {
        get => _material.GetShaderParameter("distortion_percentage").As<float>();
        set => _material.SetShaderParameter("distortion_percentage", value);
    }

    /// <summary>
    /// Shader 参数: _emission_min
    /// </summary>
    public float EmissionMin {
        get => _material.GetShaderParameter("_emission_min").As<float>();
        set => _material.SetShaderParameter("_emission_min", value);
    }

    /// <summary>
    /// Shader 参数: _emission_max
    /// </summary>
    public float EmissionMax {
        get => _material.GetShaderParameter("_emission_max").As<float>();
        set => _material.SetShaderParameter("_emission_max", value);
    }

    /// <summary>
    /// Shader 参数: _fresnel
    /// </summary>
    public float Fresnel {
        get => _material.GetShaderParameter("_fresnel").As<float>();
        set => _material.SetShaderParameter("_fresnel", value);
    }

    /// <summary>
    /// Shader 参数: viewmodel_fov
    /// </summary>
    public float viewmodelFov {
        get => _material.GetShaderParameter("viewmodel_fov").As<float>();
        set => _material.SetShaderParameter("viewmodel_fov", value);
    }

    /// <summary>
    /// Global Shader 参数: max_height
    /// </summary>
    public float maxHeight {
        get => RenderingServer.GlobalShaderParameterGet("max_height").As<float>();
        set => RenderingServer.GlobalShaderParameterSet("max_height", value);
    }

    /// <summary>
    /// Global Shader 参数: radius
    /// </summary>
    public float radius {
        get => RenderingServer.GlobalShaderParameterGet("radius").As<float>();
        set => RenderingServer.GlobalShaderParameterSet("radius", value);
    }

    /// <summary>
    /// Global Shader 参数: divisions
    /// </summary>
    public int divisions {
        get => RenderingServer.GlobalShaderParameterGet("divisions").As<int>();
        set => RenderingServer.GlobalShaderParameterSet("divisions", value);
    }

}
