using Newtonsoft.Json;
using RegisterSystem;

namespace CakeToolset.Serialize;

public class RegisterItemJsonConverter : JsonConverter {

    public RegisterSystem.RegisterSystem registerSystem { get; }

    public RegisterItemJsonConverter(RegisterSystem.RegisterSystem registerSystem) {
        this.registerSystem = registerSystem;
    }

    public override bool CanConvert(Type objectType) {
        return typeof(RegisterBasics).IsAssignableFrom(objectType);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        ResourceLocation name = ((RegisterBasics)value).name;
        serializer.Serialize(writer, name);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) {
            return null;
        }

        ResourceLocation? resourceLocation = serializer.Deserialize<ResourceLocation>(reader);
        if (resourceLocation == null) {
            return null;
        }

        RegisterManage? registerManage = registerSystem.getRegisterManageOfRegisterType(objectType);

        if (registerManage is null) {
            return null;
        }

        return registerManage.getErase(resourceLocation);
    }

}
