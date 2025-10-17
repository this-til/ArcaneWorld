using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CakeToolset.Attribute;

namespace CakeToolset.Serialize;

/// <summary>
/// 支持复杂类型键的字典转换器
/// </summary>
[JsonConverterAutomaticLoad(priority: -100)]
public class ComplexKeyDictionaryConverterFactory : JsonConverterFactory {

    public override bool CanConvert(Type typeToConvert) {
        if (!typeToConvert.IsGenericType) {
            return false;
        }

        var genericType = typeToConvert.GetGenericTypeDefinition();

        // 支持 Dictionary<TKey, TValue>
        if (genericType == typeof(Dictionary<,>)) {
            return true;
        }

        // 支持其他字典类型，如 IDictionary<,>, ConcurrentDictionary<,> 等
        return typeToConvert.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        var keyType = typeToConvert.GetGenericArguments()[0];
        var valueType = typeToConvert.GetGenericArguments()[1];

        var converterType = typeof(ComplexKeyDictionaryConverter<,>)
            .MakeGenericType(keyType, valueType);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

}

/// <summary>
/// 复杂键字典的具体转换器实现
/// </summary>
public class ComplexKeyDictionaryConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
    where TKey : notnull {

    public override Dictionary<TKey, TValue>? Read
    (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) {
        if (reader.TokenType == JsonTokenType.Null) {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException($"期望 StartObject 令牌，但得到 {reader.TokenType}");
        }

        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                return dictionary;
            }

            // 读取属性名（键的序列化字符串）
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException($"期望 PropertyName 令牌，但得到 {reader.TokenType}");
            }

            string? keyString = reader.GetString();
            if (keyString == null) {
                throw new JsonException("字典的键不能为 null");
            }

            // 反序列化键：将字符串包装成 JSON 字符串令牌传给系统
            TKey key = DeserializeKey(keyString, options);

            // 读取值
            reader.Read();
            TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);

            dictionary.Add(key, value!);
        }

        throw new JsonException("JSON 格式不正确：未找到对象结束标记");
    }

    public override void Write
    (
        Utf8JsonWriter writer,
        Dictionary<TKey, TValue> dictionary,
        JsonSerializerOptions options
    ) {
        writer.WriteStartObject();

        foreach (var kvp in dictionary) {
            // 序列化键为字符串属性名
            string keyString = SerializeKey(kvp.Key, options);
            writer.WritePropertyName(keyString);

            // 序列化值
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// 序列化键为 JSON 字符串
    /// 只允许值类型的键，复杂结构会抛出异常
    /// </summary>
    private string SerializeKey(TKey key, JsonSerializerOptions options) {
        // 如果键本身就是字符串类型，直接返回
        if (key is string str) {
            return str;
        }

        // 序列化为 JsonNode 进行类型检查
        var jsonNode = JsonSerializer.SerializeToNode(key, options);

        if (jsonNode == null) {
            throw new JsonException($"字典的键 {key} 序列化后为 null");
        }

        // 检查是否为值类型（JsonValue）
        if (jsonNode is JsonValue) {
            // 值类型：提取字符串表示
            return jsonNode.ToJsonString().Trim('"');
        }

        throw new JsonException(
            $"字典的键类型不支持。键类型: {typeof(TKey).Name}, " +
            $"JsonNode 类型: {jsonNode.GetType().Name}"
        );
    }

    /// <summary>
    /// 从 JSON 字符串反序列化键
    /// 添加 "" 来伪造一个 String 令牌传入系统
    /// </summary>
    private TKey DeserializeKey(string keyString, JsonSerializerOptions options) {
        // 如果键类型是字符串，直接返回
        if (typeof(TKey) == typeof(string)) {
            return (TKey)(object)keyString;
        }

        try {
            // 需要伪造成 JSON 字符串令牌
            string fakeJsonToken = $"\"{keyString}\"";
            return JsonSerializer.Deserialize<TKey>(fakeJsonToken, options)!;
        }
        catch {
            // 如果失败，尝试使用 Convert
            return (TKey)Convert.ChangeType(keyString, typeof(TKey));
        }
    }

}
