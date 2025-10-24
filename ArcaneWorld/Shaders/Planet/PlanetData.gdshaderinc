// 星球数据着色器包含文件
// 包含星球相关的常量和参数

// 星球参数（从C#代码传入）
uniform float radius = 100.0;
uniform float atmosphereHeight = 120; 
uniform int chunkDivisions = 5;
uniform int tileDivisions = 50;

uniform mat4 invPlanetMatrix;