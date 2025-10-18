using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader 包装类: LineAlpha.gdshader
/// 路径: res://Shaders/Planet/LongitudeLatitude/LineAlpha.gdshader
/// </summary>
public class LineAlphaShader {

    private readonly ShaderMaterial _material;

    public LineAlphaShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: alpha_factor
    /// </summary>
    public float alphaFactor {
        get => _material.GetShaderParameter("alpha_factor").As<float>();
        set => _material.SetShaderParameter("alpha_factor", value);
    }

    /// <summary>
    /// Shader 参数: albedo
    /// </summary>
    public Color albedo {
        get => _material.GetShaderParameter("albedo").As<Color>();
        set => _material.SetShaderParameter("albedo", value);
    }

    /// <summary>
    /// Shader 参数: texture_albedo
    /// </summary>
    public Color textureAlbedo {
        get => _material.GetShaderParameter("texture_albedo").As<Color>();
        set => _material.SetShaderParameter("texture_albedo", value);
    }

    /// <summary>
    /// Shader 参数: albedo_texture_size
    /// </summary>
    public Vector2I albedoTextureSize {
        get => _material.GetShaderParameter("albedo_texture_size").As<Vector2I>();
        set => _material.SetShaderParameter("albedo_texture_size", value);
    }

    /// <summary>
    /// Shader 参数: point_size
    /// </summary>
    public float pointSize {
        get => _material.GetShaderParameter("point_size").As<float>();
        set => _material.SetShaderParameter("point_size", value);
    }

    /// <summary>
    /// Shader 参数: roughness
    /// </summary>
    public float roughness {
        get => _material.GetShaderParameter("roughness").As<float>();
        set => _material.SetShaderParameter("roughness", value);
    }

    /// <summary>
    /// Shader 参数: texture_metallic
    /// </summary>
    public Texture2D textureMetallic {
        get => _material.GetShaderParameter("texture_metallic").As<Texture2D>();
        set => _material.SetShaderParameter("texture_metallic", value);
    }

    /// <summary>
    /// Shader 参数: metallic_texture_channel
    /// </summary>
    public Vector4 metallicTextureChannel {
        get => _material.GetShaderParameter("metallic_texture_channel").As<Vector4>();
        set => _material.SetShaderParameter("metallic_texture_channel", value);
    }

    /// <summary>
    /// Shader 参数: texture_roughness
    /// </summary>
    public Texture2D textureRoughness {
        get => _material.GetShaderParameter("texture_roughness").As<Texture2D>();
        set => _material.SetShaderParameter("texture_roughness", value);
    }

    /// <summary>
    /// Shader 参数: specular
    /// </summary>
    public float specular {
        get => _material.GetShaderParameter("specular").As<float>();
        set => _material.SetShaderParameter("specular", value);
    }

    /// <summary>
    /// Shader 参数: metallic
    /// </summary>
    public float metallic {
        get => _material.GetShaderParameter("metallic").As<float>();
        set => _material.SetShaderParameter("metallic", value);
    }

    /// <summary>
    /// Shader 参数: uv1_scale
    /// </summary>
    public Vector3 uv1Scale {
        get => _material.GetShaderParameter("uv1_scale").As<Vector3>();
        set => _material.SetShaderParameter("uv1_scale", value);
    }

    /// <summary>
    /// Shader 参数: uv1_offset
    /// </summary>
    public Vector3 uv1Offset {
        get => _material.GetShaderParameter("uv1_offset").As<Vector3>();
        set => _material.SetShaderParameter("uv1_offset", value);
    }

    /// <summary>
    /// Shader 参数: uv2_scale
    /// </summary>
    public Vector3 uv2Scale {
        get => _material.GetShaderParameter("uv2_scale").As<Vector3>();
        set => _material.SetShaderParameter("uv2_scale", value);
    }

    /// <summary>
    /// Shader 参数: uv2_offset
    /// </summary>
    public Vector3 uv2Offset {
        get => _material.GetShaderParameter("uv2_offset").As<Vector3>();
        set => _material.SetShaderParameter("uv2_offset", value);
    }

}
