using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: PlanetSurface.gdshader
/// 路径: res://Shaders/Planet/PlanetSurface.gdshader
/// </summary>
public class PlanetSurfaceShader {

    private readonly ShaderMaterial _material;

    public PlanetSurfaceShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: albedo_texture
    /// </summary>
    public Color albedoTexture {
        get => _material.GetShaderParameter("albedo_texture").As<Color>();
        set => _material.SetShaderParameter("albedo_texture", value);
    }

    /// <summary>
    /// Shader 参数: normal_texture
    /// </summary>
    public Texture2D normalTexture {
        get => _material.GetShaderParameter("normal_texture").As<Texture2D>();
        set => _material.SetShaderParameter("normal_texture", value);
    }

    /// <summary>
    /// Shader 参数: metallic_texture
    /// </summary>
    public Texture2D metallicTexture {
        get => _material.GetShaderParameter("metallic_texture").As<Texture2D>();
        set => _material.SetShaderParameter("metallic_texture", value);
    }

    /// <summary>
    /// Shader 参数: roughness_texture
    /// </summary>
    public Texture2D roughnessTexture {
        get => _material.GetShaderParameter("roughness_texture").As<Texture2D>();
        set => _material.SetShaderParameter("roughness_texture", value);
    }

    /// <summary>
    /// Shader 参数: texture_scale
    /// </summary>
    public float textureScale {
        get => _material.GetShaderParameter("texture_scale").As<float>();
        set => _material.SetShaderParameter("texture_scale", value);
    }

    /// <summary>
    /// Shader 参数: triplanar_scale
    /// </summary>
    public float triplanarScale {
        get => _material.GetShaderParameter("triplanar_scale").As<float>();
        set => _material.SetShaderParameter("triplanar_scale", value);
    }

    /// <summary>
    /// Shader 参数: albedo_color
    /// </summary>
    public Color albedoColor {
        get => _material.GetShaderParameter("albedo_color").As<Color>();
        set => _material.SetShaderParameter("albedo_color", value);
    }

    /// <summary>
    /// Shader 参数: metallic
    /// </summary>
    public float metallic {
        get => _material.GetShaderParameter("metallic").As<float>();
        set => _material.SetShaderParameter("metallic", value);
    }

    /// <summary>
    /// Shader 参数: roughness
    /// </summary>
    public float roughness {
        get => _material.GetShaderParameter("roughness").As<float>();
        set => _material.SetShaderParameter("roughness", value);
    }

    /// <summary>
    /// Shader 参数: normal_strength
    /// </summary>
    public float normalStrength {
        get => _material.GetShaderParameter("normal_strength").As<float>();
        set => _material.SetShaderParameter("normal_strength", value);
    }

    /// <summary>
    /// Shader 参数: use_height_coloring
    /// </summary>
    public bool useHeightColoring {
        get => _material.GetShaderParameter("use_height_coloring").As<bool>();
        set => _material.SetShaderParameter("use_height_coloring", value);
    }

    /// <summary>
    /// Shader 参数: height_color_strength
    /// </summary>
    public float heightColorStrength {
        get => _material.GetShaderParameter("height_color_strength").As<float>();
        set => _material.SetShaderParameter("height_color_strength", value);
    }

    /// <summary>
    /// Shader 参数: low_color
    /// </summary>
    public Color lowColor {
        get => _material.GetShaderParameter("low_color").As<Color>();
        set => _material.SetShaderParameter("low_color", value);
    }

    /// <summary>
    /// Shader 参数: high_color
    /// </summary>
    public Color highColor {
        get => _material.GetShaderParameter("high_color").As<Color>();
        set => _material.SetShaderParameter("high_color", value);
    }

}
