using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: RiverUV.gdshader
/// 路径: res://Shaders/Planet/RiverUV.gdshader
/// </summary>
public class RiverUVShader {

    private readonly ShaderMaterial _material;

    public RiverUVShader(ShaderMaterial material) {
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
    /// Shader 参数: metallic
    /// </summary>
    public float metallic {
        get => _material.GetShaderParameter("metallic").As<float>();
        set => _material.SetShaderParameter("metallic", value);
    }

}
