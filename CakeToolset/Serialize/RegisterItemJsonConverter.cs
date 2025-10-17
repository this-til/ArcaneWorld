using System.Text.Json;
using System.Text.Json.Serialization;
using CakeToolset.Attribute;
using CakeToolset.Global.Component;
using RegisterSystem;

namespace CakeToolset.Serialize;

[JsonConverterAutomaticLoad]
public class RegisterItemJsonConverterFactory : JsonConverterFactory {

    public override bool CanConvert(Type typeToConvert) {
        return typeof(RegisterBasics).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        return (JsonConverter)Activator.CreateInstance(
            typeof(RegisterItemJsonConverter<>).MakeGenericType(typeToConvert))!;
    }
}

public class RegisterItemJsonConverter<T> : JsonConverter<T> where T : RegisterBasics {

    public RegisterSystem.RegisterSystem registerSystem => RegisterSystemHold.registerSystem;

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        if (value == null) {
            writer.WriteNullValue();
            return;
        }

        ResourceLocation name = value.name;
        JsonSerializer.Serialize(writer, name, options);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.Null) {
            return null;
        }

        ResourceLocation? resourceLocation = JsonSerializer.Deserialize<ResourceLocation>(ref reader, options);
        if (resourceLocation == null) {
            return null;
        }

        RegisterManage? registerManage = registerSystem.getRegisterManageOfRegisterType(typeToConvert);

        if (registerManage is null) {
            return null;
        }

        return (T?)registerManage.getErase(resourceLocation);
    }

}
