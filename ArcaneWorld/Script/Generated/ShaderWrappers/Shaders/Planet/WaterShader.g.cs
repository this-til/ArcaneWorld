using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: Water.gdshader
/// 路径: res://Shaders/Planet/Water.gdshader
/// </summary>
public class WaterShader {

    private readonly ShaderMaterial _material;

    public WaterShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: DEPTH_TEXTURE
    /// </summary>
    public Texture2D dEPTHTEXTURE {
        get => _material.GetShaderParameter("DEPTH_TEXTURE").As<Texture2D>();
        set => _material.SetShaderParameter("DEPTH_TEXTURE", value);
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
    /// Shader 参数: deep_col
    /// </summary>
    public Color deepCol {
        get => _material.GetShaderParameter("deep_col").As<Color>();
        set => _material.SetShaderParameter("deep_col", value);
    }

    /// <summary>
    /// Shader 参数: shallow_col
    /// </summary>
    public Color shallowCol {
        get => _material.GetShaderParameter("shallow_col").As<Color>();
        set => _material.SetShaderParameter("shallow_col", value);
    }

    /// <summary>
    /// Shader 参数: wave_normal_A
    /// </summary>
    public Texture2D waveNormalA {
        get => _material.GetShaderParameter("wave_normal_A").As<Texture2D>();
        set => _material.SetShaderParameter("wave_normal_A", value);
    }

    /// <summary>
    /// Shader 参数: wave_normal_B
    /// </summary>
    public Texture2D waveNormalB {
        get => _material.GetShaderParameter("wave_normal_B").As<Texture2D>();
        set => _material.SetShaderParameter("wave_normal_B", value);
    }

    /// <summary>
    /// Shader 参数: foam_noise_tex
    /// </summary>
    public Texture2D foamNoiseTex {
        get => _material.GetShaderParameter("foam_noise_tex").As<Texture2D>();
        set => _material.SetShaderParameter("foam_noise_tex", value);
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
    /// Global Shader 参数: dir_to_sun
    /// </summary>
    public Vector3 dirToSun {
        get => RenderingServer.GlobalShaderParameterGet("dir_to_sun").As<Vector3>();
        set => RenderingServer.GlobalShaderParameterSet("dir_to_sun", value);
    }

    /// <summary>
    /// Global Shader 参数: inv_planet_matrix
    /// </summary>
    public Transform3D invPlanetMatrix {
        get => RenderingServer.GlobalShaderParameterGet("inv_planet_matrix").As<Transform3D>();
        set => RenderingServer.GlobalShaderParameterSet("inv_planet_matrix", value);
    }

    /// <summary>
    /// Shader 参数: fresnel
    /// </summary>
    public float fresnel {
        get => _material.GetShaderParameter("fresnel").As<float>();
        set => _material.SetShaderParameter("fresnel", value);
    }

    /// <summary>
    /// Shader 参数: shore_fade
    /// </summary>
    public float shoreFade {
        get => _material.GetShaderParameter("shore_fade").As<float>();
        set => _material.SetShaderParameter("shore_fade", value);
    }

}
