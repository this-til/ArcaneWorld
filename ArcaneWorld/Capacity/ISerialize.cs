using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArcaneWorld.Capacity;

public interface ISerialize<J> where J : JContainer {

    public J serialize(JsonSerializer jsonSerializer);

    public void deserialize(J data, JsonSerializer jsonSerializer);

}
