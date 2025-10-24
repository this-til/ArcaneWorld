using Godot;
using System;

namespace ArcaneWorld.Generated.ShaderWrappers;

/// <summary>
/// Shader Include 包装类: cloud_funcs.gdshaderinc
/// 路径: res://addons/zylann.atmosphere/shaders/include/cloud_funcs.gdshaderinc
/// </summary>
public class CloudFuncsIncShader {

    private readonly ShaderMaterial _material;

    public CloudFuncsIncShader(ShaderMaterial material) {
        _material = material;
    }

    /// <summary>
    /// Shader 参数: u_cloud_density_scale
    /// </summary>
    public float uCloudDensityScale {
        get => _material.GetShaderParameter("u_cloud_density_scale").As<float>();
        set => _material.SetShaderParameter("u_cloud_density_scale", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_bottom
    /// </summary>
    public float uCloudBottom {
        get => _material.GetShaderParameter("u_cloud_bottom").As<float>();
        set => _material.SetShaderParameter("u_cloud_bottom", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_top
    /// </summary>
    public float uCloudTop {
        get => _material.GetShaderParameter("u_cloud_top").As<float>();
        set => _material.SetShaderParameter("u_cloud_top", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_blend
    /// </summary>
    public float uCloudBlend {
        get => _material.GetShaderParameter("u_cloud_blend").As<float>();
        set => _material.SetShaderParameter("u_cloud_blend", value);
    }

    /// <summary>
    /// Shader 参数: u_world_to_model_matrix
    /// </summary>
    public Transform3D uWorldToModelMatrix {
        get => _material.GetShaderParameter("u_world_to_model_matrix").As<Transform3D>();
        set => _material.SetShaderParameter("u_world_to_model_matrix", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_shape_texture
    /// </summary>
    public Texture3D uCloudShapeTexture {
        get => _material.GetShaderParameter("u_cloud_shape_texture").As<Texture3D>();
        set => _material.SetShaderParameter("u_cloud_shape_texture", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_shape_invert
    /// </summary>
    public float uCloudShapeInvert {
        get => _material.GetShaderParameter("u_cloud_shape_invert").As<float>();
        set => _material.SetShaderParameter("u_cloud_shape_invert", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_coverage_bias
    /// </summary>
    public float uCloudCoverageBias {
        get => _material.GetShaderParameter("u_cloud_coverage_bias").As<float>();
        set => _material.SetShaderParameter("u_cloud_coverage_bias", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_shape_factor
    /// </summary>
    public float uCloudShapeFactor {
        get => _material.GetShaderParameter("u_cloud_shape_factor").As<float>();
        set => _material.SetShaderParameter("u_cloud_shape_factor", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_shape_scale
    /// </summary>
    public float uCloudShapeScale {
        get => _material.GetShaderParameter("u_cloud_shape_scale").As<float>();
        set => _material.SetShaderParameter("u_cloud_shape_scale", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_coverage_cubemap
    /// </summary>
    public Cubemap uCloudCoverageCubemap {
        get => _material.GetShaderParameter("u_cloud_coverage_cubemap").As<Cubemap>();
        set => _material.SetShaderParameter("u_cloud_coverage_cubemap", value);
    }

    /// <summary>
    /// Shader 参数: u_cloud_coverage_rotation
    /// </summary>
    public Vector4 uCloudCoverageRotation {
        get => _material.GetShaderParameter("u_cloud_coverage_rotation").As<Vector4>();
        set => _material.SetShaderParameter("u_cloud_coverage_rotation", value);
    }

}
