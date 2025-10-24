using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: Wall.gdshader
/// 路径: res://Shaders/Planet/Wall.gdshader
/// </summary>
public class WallShader {

    private readonly ShaderMaterial _material;

    public WallShader(ShaderMaterial material) {
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
    public Texture2D mainTex {
        get => _material.GetShaderParameter("main_tex").As<Texture2D>();
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

}
