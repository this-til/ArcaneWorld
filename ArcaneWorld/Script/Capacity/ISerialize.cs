using System.Text.Json;
using System.Text.Json.Nodes;

namespace ArcaneWorld.Capacity;

public interface ISerialize<J> : ILock where J : JsonNode {

    public J serialize(JsonSerializerOptions jsonSerializerOptions);

    public void deserialize(J data, JsonSerializerOptions jsonSerializerOptions);

}

public interface IAutoSerialize : ISerialize<JsonObject> {

}
