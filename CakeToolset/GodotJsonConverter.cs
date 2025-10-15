using Godot;
using Godot.Collections;
using Newtonsoft.Json.Linq;
using Array = Godot.Collections.Array;

namespace CakeToolset.Serialize;

/// <summary>
/// Godot Dictionary 和 Newtonsoft.Json 之间的转换扩展方法
/// </summary>
public static class GodotJsonConverter {

    /// <summary>
    /// 将 Godot Dictionary 转换为 JObject
    /// </summary>
    public static JObject ToJObject(this Dictionary godotDict) {
        if (godotDict == null) {
            return new JObject();
        }

        JObject jObject = new JObject();
        foreach (Variant key in godotDict.Keys) {
            string keyStr = key.AsString();
            JToken value = godotDict[key].ToJToken();
            jObject[keyStr] = value;
        }
        return jObject;
    }

    /// <summary>
    /// 将 Godot Array 转换为 JArray
    /// </summary>
    public static JArray ToJArray(this Array godotArray) {
        if (godotArray == null) {
            return new JArray();
        }

        JArray jArray = new JArray();
        foreach (Variant item in godotArray) {
            jArray.Add(item.ToJToken());
        }
        return jArray;
    }

    /// <summary>
    /// 将 Godot Variant 转换为 JToken
    /// </summary>
    public static JToken ToJToken(this Variant variant) {
        if (variant.Obj == null) {
            return JValue.CreateNull();
        }

        return variant.VariantType switch {
            Variant.Type.Nil => JValue.CreateNull(),
            Variant.Type.Bool => new JValue(variant.AsBool()),
            Variant.Type.Int => new JValue(variant.AsInt64()),
            Variant.Type.Float => new JValue(variant.AsDouble()),
            Variant.Type.String => new JValue(variant.AsString()),
            Variant.Type.Dictionary => variant.AsGodotDictionary().ToJObject(),
            Variant.Type.Array => variant.AsGodotArray().ToJArray(),

            // 向量类型转换为数组
            Variant.Type.Vector2 => variant.AsVector2().ToJArray(),
            Variant.Type.Vector2I => variant.AsVector2I().ToJArray(),
            Variant.Type.Vector3 => variant.AsVector3().ToJArray(),
            Variant.Type.Vector3I => variant.AsVector3I().ToJArray(),
            Variant.Type.Vector4 => variant.AsVector4().ToJArray(),
            Variant.Type.Vector4I => variant.AsVector4I().ToJArray(),

            // 颜色转换为对象
            Variant.Type.Color => variant.AsColor().ToJObject(),

            // 其他类型转换为字符串
            _ => new JValue(variant.ToString())
        };
    }

    /// <summary>
    /// 将 JObject 转换为 Godot Dictionary
    /// </summary>
    public static Dictionary ToGodotDictionary(this JObject jObject) {
        if (jObject == null) {
            return new Dictionary();
        }

        Dictionary dictionary = new Dictionary();
        foreach (JProperty property in jObject.Properties()) {
            dictionary[property.Name] = property.Value.ToVariant();
        }
        return dictionary;
    }

    /// <summary>
    /// 将 JArray 转换为 Godot Array
    /// </summary>
    public static Array ToGodotArray(this JArray jArray) {
        if (jArray == null) {
            return new Array();
        }

        Array array = new Array();
        foreach (JToken item in jArray) {
            array.Add(item.ToVariant());
        }
        return array;
    }

    /// <summary>
    /// 将 JToken 转换为 Godot Variant
    /// </summary>
    public static Variant ToVariant(this JToken jToken) {
        if (jToken == null || jToken.Type == JTokenType.Null) {
            return default(Variant);
        }

        return jToken.Type switch {
            JTokenType.Object => Variant.From(((JObject)jToken).ToGodotDictionary()),
            JTokenType.Array => Variant.From(((JArray)jToken).ToGodotArray()),
            JTokenType.Integer => Variant.From(jToken.Value<long>()),
            JTokenType.Float => Variant.From(jToken.Value<double>()),
            JTokenType.String => Variant.From(jToken.Value<string>()),
            JTokenType.Boolean => Variant.From(jToken.Value<bool>()),
            JTokenType.Date => Variant.From(jToken.Value<DateTime>().ToString("yyyy-MM-dd HH:mm:ss")),
            JTokenType.Bytes => Variant.From(Convert.ToBase64String(jToken.Value<byte[]>() ?? [])),
            _ => Variant.From(jToken.ToString())
        };
    }

    /// <summary>
    /// 安全设置 Dictionary 的值
    /// </summary>
    public static Dictionary SetValue<T>(this Dictionary dict, string key, T value) {
        dict ??= new Dictionary();
        dict[key] = Variant.From(value);
        return dict;
    }

    // 向量类型扩展方法
    public static JArray ToJArray(this Vector2 vector) {
        return new JArray(vector.X, vector.Y);
    }

    public static JArray ToJArray(this Vector2I vector) {
        return new JArray(vector.X, vector.Y);
    }

    public static JArray ToJArray(this Vector3 vector) {
        return new JArray(vector.X, vector.Y, vector.Z);
    }

    public static JArray ToJArray(this Vector3I vector) {
        return new JArray(vector.X, vector.Y, vector.Z);
    }

    public static JArray ToJArray(this Vector4 vector) {
        return new JArray(vector.X, vector.Y, vector.Z, vector.W);
    }

    public static JArray ToJArray(this Vector4I vector) {
        return new JArray(vector.X, vector.Y, vector.Z, vector.W);
    }

    public static JObject ToJObject(this Color color) {
        return new JObject {
            ["r"] = color.R,
            ["g"] = color.G,
            ["b"] = color.B,
            ["a"] = color.A
        };
    }

}
