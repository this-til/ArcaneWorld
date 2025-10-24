shader_type spatial;
render_mode depth_prepass_alpha;

#include "PlanetData.gdshaderinc"
#include "Sebastian/Triplanar.gdshaderinc"

// 贴图参数
uniform sampler2D albedo_texture : source_color, filter_linear_mipmap;
uniform sampler2D normal_texture : hint_normal, filter_linear_mipmap;
uniform sampler2D metallic_texture : hint_default_white, filter_linear_mipmap;
uniform sampler2D roughness_texture : hint_roughness_r, filter_linear_mipmap;

// 材质参数
uniform float triplanar_scale : hint_range(0.1, 50.0) = 1.0;
uniform vec4 albedo_color : source_color = vec4(1.0);

varying vec3 world_position;
varying vec3 world_normal;
varying vec3 local_position;
varying vec3 local_normal;
varying vec4 tangent_vector;


void vertex() {
    // 计算世界坐标和世界法线
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);
    
    // 计算本地坐标和本地法线（用于 triplanar 映射）
    local_position = VERTEX;
    local_normal = normalize(NORMAL);
    
    // 传递切线向量 (扩展为 vec4，w 分量为 handedness)
    tangent_vector = vec4(TANGENT, 1.0);

}

void fragment() {
    // 使用 triplanar 映射采样贴图
    vec4 albedo_tex = triplanar(local_position, local_normal, triplanar_scale, albedo_texture);
    vec3 normal_tex = triplanar(local_position, local_normal, triplanar_scale, normal_texture).rgb;
    float metallic_tex = triplanar(local_position, local_normal, triplanar_scale, metallic_texture).r;
    float roughness_tex = triplanar(local_position, local_normal, triplanar_scale, roughness_texture).r;
    
    // 计算最终反照率
    vec4 final_albedo = albedo_tex * albedo_color;
    
    
    // 输出材质属性
    ALBEDO = final_albedo.rgb;
    ALPHA = final_albedo.a;
    METALLIC = metallic_tex;
    ROUGHNESS = roughness_tex;
    NORMAL_MAP = normal_tex;
}