using ArcaneWorld.Attribute;
using ArcaneWorld.Capacity;
using ArcaneWorld.Capacity.Instance;
using ArcaneWorld.Global;
using ArcaneWorld.Register;
using ArcaneWorld.Util;
using CakeToolset.Serialize;
using FlexibleRequired;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArcaneWorld.Mechanical;

[ClassName]
public partial class MechanicalBase : Node3D, IAutoSerialize {

    protected ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    [Required]
    [SaveField]
    public string owner {
        get {
            using (lockForRead()) {
                return field;
            }
        }
        set {
            using (lockForWrite()) {
                field = value;
            }
        }
    } = string.Empty;

    [SaveField]
    public Guid mechanicalId {
        get {
            using (lockForRead()) {
                return field;
            }
        }
        set {
            using (lockForWrite()) {
                field = value;
            }
        }
    } = Guid.NewGuid();
    
    [Export(PropertyHint.MultilineText)]
    public String saveData {
        get => serialize(JsonSerializerHold.jsonSerializer).ToString();
        set {
            JObject jObject = JsonSerializerHold.jsonSerializer.Deserialize<JObject>(new JsonTextReader(new StringReader(value)))!;
            deserialize(jObject, JsonSerializerHold.jsonSerializer);
        }
    }

    public StructReadLockContext lockForRead() {
        return new StructReadLockContext(rwLock);
    }

    public StructWriteLockContext lockForWrite() {
        return new StructWriteLockContext(rwLock);
    }

    protected partial void onBeforeSerialize(JsonSerializer jsonSerializer) { }
    protected partial void onAfterSerialize(JObject jObject, JsonSerializer jsonSerializer) { }
    protected partial void onBeforeDeserialize(JObject data, JsonSerializer jsonSerializer) { }
    protected partial void onAfterDeserialize(JObject data, JsonSerializer jsonSerializer) { }

}
