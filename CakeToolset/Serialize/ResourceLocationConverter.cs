using System.Text.Json;
using System.Text.Json.Serialization;
using CakeToolset.Attribute;
using RegisterSystem;

namespace CakeToolset.Serialize;

/// <summary>
/// ResourceLocation 的 JSON 转换器
/// 序列化为单一字符串格式 "domain:path"
/// </summary>
[JsonConverterAutomaticLoad]
public class ResourceLocationConverter : JsonConverter<ResourceLocation> {

    public override ResourceLocation? Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options) 
    {
        if (reader.TokenType == JsonTokenType.Null) {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String) {
            throw new JsonException(
                $"期望 String 令牌来反序列化 ResourceLocation，但得到 {reader.TokenType}"
            );
        }

        string? locationString = reader.GetString();
        
        if (string.IsNullOrEmpty(locationString)) {
            throw new JsonException("ResourceLocation 字符串不能为空");
        }

        try {
            // 使用 ResourceLocation 的字符串构造函数
            return new ResourceLocation(locationString);
        }
        catch (ArgumentException ex) {
            throw new JsonException(
                $"无法从字符串 '{locationString}' 创建 ResourceLocation: {ex.Message}", 
                ex
            );
        }
    }

    public override void Write(
        Utf8JsonWriter writer, 
        ResourceLocation value, 
        JsonSerializerOptions options) 
    {
        if (value == null) {
            writer.WriteNullValue();
            return;
        }

        // 使用 ToString() 方法获取 "domain:path" 格式的字符串
        writer.WriteStringValue(value.ToString());
    }
}

