using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: atmosphere_funcs_v2.gdshaderinc
/// 路径: res://addons/zylann.atmosphere/shaders/include/atmosphere_funcs_v2.gdshaderinc
/// </summary>
public class AtmosphereFuncsV2IncShader {

    private readonly ShaderMaterial _material;

    public AtmosphereFuncsV2IncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: u_optical_depth_texture
    /// </summary>
    public Texture2D uOpticalDepthTexture {
        get => _material.GetShaderParameter("u_optical_depth_texture").As<Texture2D>();
        set => _material.SetShaderParameter("u_optical_depth_texture", value);
    }

    /// <summary>
    /// Shader 参数: u_scattering_strength
    /// </summary>
    public float uScatteringStrength {
        get => _material.GetShaderParameter("u_scattering_strength").As<float>();
        set => _material.SetShaderParameter("u_scattering_strength", value);
    }

    /// <summary>
    /// Shader 参数: u_scattering_wavelengths
    /// </summary>
    public Vector3 uScatteringWavelengths {
        get => _material.GetShaderParameter("u_scattering_wavelengths").As<Vector3>();
        set => _material.SetShaderParameter("u_scattering_wavelengths", value);
    }

    /// <summary>
    /// Shader 参数: u_atmosphere_modulate
    /// </summary>
    public Color uAtmosphereModulate {
        get => _material.GetShaderParameter("u_atmosphere_modulate").As<Color>();
        set => _material.SetShaderParameter("u_atmosphere_modulate", value);
    }

    /// <summary>
    /// Shader 参数: u_atmosphere_ambient_color
    /// </summary>
    public Color uAtmosphereAmbientColor {
        get => _material.GetShaderParameter("u_atmosphere_ambient_color").As<Color>();
        set => _material.SetShaderParameter("u_atmosphere_ambient_color", value);
    }

}
