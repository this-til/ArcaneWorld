shader_type spatial;

#include "PlanetData.gdshaderinc"
#include "Sebastian/Triplanar.gdshaderinc"

uniform vec3 albedo : source_color;
uniform vec3 fresnelColor : source_color;
uniform float fresnelPower : hint_range(1.0, 10.0) = 5.0;
uniform float metallic : hint_range(0.0, 1.0) = 0;
uniform float roughness : hint_range(0.0, 1.0) = 0.02;

uniform float triplanarScale : hint_range(0.001, 50.0) = 1.0;

uniform sampler2D textureNormal;
uniform vec2 waveDirection = vec2(2.0, 0);

uniform sampler2D textureNormal2;
uniform vec2 waveDirection2 = vec2(0, 1.0);

uniform float timeScale : hint_range(0.0, 0.20, 0.005) = 0.025;

uniform sampler2D wave;
uniform float noiseScale = 10;
uniform float heightScale = 0.15;

uniform vec3 deepColor : source_color;
uniform vec3 shallowColor : source_color;
uniform float beersLaw = 2.0;
uniform float deepOffset = -0.75;


// 水下渲染参数
uniform float waterTransparency : hint_range(0.0, 1.0) = 0.7;
uniform float depthScale : hint_range(0.1, 10.0) = 1.0;
uniform sampler2D DEPTH_TEXTURE : source_color, hint_depth_texture, filter_linear_mipmap;
uniform sampler2D SCREEN_TEXTURE : source_color, hint_screen_texture, filter_linear_mipmap;

uniform vec3 edgeColor : source_color;
uniform float edgeScale = 0.1;
uniform float near = 1.0;
uniform float far = 100;

varying vec3 world_position;
varying vec3 world_normal;
varying vec3 local_position;
varying vec3 local_normal;
varying vec4 tangent_vector;
varying float height;

float fresnel(float amount, vec3 normal, vec3 view){
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0)), amount);
}

float edge(float depth){
	depth = 1.0 - 2.0 * depth;
	return near * far / (far + depth * (near-far));
}

void vertex() {
    // 计算世界坐标和世界法线
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);

    // 计算本地坐标和本地法线（用于 triplanar 映射）
    local_position = VERTEX;
    local_normal = normalize(NORMAL);

    // 传递切线向量 (扩展为 vec4，w 分量为 handedness)
    tangent_vector = vec4(TANGENT, 1.0);

	height = texture(wave, world_position.xz / noiseScale + TIME * timeScale).r;
	
	// 计算从星球中心到顶点的方向向量（法线方向）
	vec3 vertex_normal = normalize(VERTEX);
	
	// 沿着法线方向应用高度偏移，而不是简单的 Y 轴偏移
	VERTEX += vertex_normal * height * heightScale;
}


void fragment(){
	vec2 time = (TIME * waveDirection) * timeScale;
	vec2 time2 = (TIME * waveDirection2) * timeScale;

	// 计算法线贴图混合
	vec3 normal1 = triplanarOffset(local_position, local_normal, triplanarScale, time, textureNormal).rgb;
	vec3 normal2 = triplanarOffset(local_position, local_normal, triplanarScale, time2, textureNormal2).rgb;
	vec3 normalBlend = mix(normal1, normal2, 0.5);

	// 计算菲涅尔反射
	float fresnelValue = fresnel(fresnelPower, NORMAL, VIEW);
	
	// 计算深度信息用于水下效果
	float depth = texture(DEPTH_TEXTURE, SCREEN_UV).r;
	vec4 world_pos = INV_PROJECTION_MATRIX * (vec4(SCREEN_UV, depth, 1.0) * 2.0 - 1.0);
	world_pos.xyz /= world_pos.w;
	float depth_blend = exp(-world_pos.z * depthScale);
	depth_blend = clamp(depth_blend, 0.0, 1.0);
	
	// 应用Beer定律计算光线在水中的衰减
	float beer_factor = exp(-beersLaw * depth_blend);
	
	// 基于深度混合深水和浅水颜色
	vec3 water_color = mix(deepColor, shallowColor, depth_blend);
	
	// 计算水面颜色（考虑菲涅尔效应）
	vec3 surface_color = mix(albedo, fresnelColor, fresnelValue);
	
	// 折射效果：使用法线扰动来偏移屏幕UV
	vec2 distorted_uv = SCREEN_UV + (normalBlend.xy - 0.5) * 0.05;
	distorted_uv = clamp(distorted_uv, vec2(0.0), vec2(1.0));
	vec3 refracted_color = texture(SCREEN_TEXTURE, distorted_uv).rgb;
	
	// 基于深度和Beer定律混合折射和水色
	refracted_color = mix(refracted_color, water_color, 1.0 - beer_factor);
	
	// 最终颜色：基于菲涅尔混合表面反射和折射
	vec3 final_color = mix(refracted_color, surface_color, fresnelValue);
	
	// 计算透明度：深水更不透明，浅水更透明
	float final_alpha = mix(waterTransparency, 1.0, depth_blend * (1.0 - beer_factor));
	final_alpha = mix(final_alpha, 1.0, fresnelValue * 0.5);

	// 白边效果计算
	float zDepth = edge(texture(DEPTH_TEXTURE, SCREEN_UV).x);
	float zPos = edge(FRAGCOORD.z);
	float zDif = zDepth - zPos;
	
	// 计算白边强度：当水面靠近物体时产生泡沫效果
	float edge_intensity = 1.0 - smoothstep(0.0, edgeScale, zDif);
	edge_intensity = clamp(edge_intensity, 0.0, 1.0);
	
	// 将白边颜色混合到最终颜色中
	final_color = mix(final_color, edgeColor, edge_intensity);
	
	// 白边区域增加不透明度，模拟泡沫效果
	final_alpha = mix(final_alpha, 1.0, edge_intensity * 0.8);

	ALBEDO = final_color;
	ALPHA = final_alpha;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	NORMAL_MAP = normalBlend;
}