using System.Text.Json;
using System.Text.Json.Nodes;
using ArcaneWorld.Attribute;
using ArcaneWorld.Capacity;
using ArcaneWorld.Capacity.Instance;
using ArcaneWorld.Global;
using ArcaneWorld.Register;
using ArcaneWorld.Util;
using CakeToolset.Global.Component;
using FlexibleRequired;
using Godot;
using Godot.Collections;

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
        get => serialize(JsonSerializerHold.jsonSerializerOptions).ToJsonString();
        set {
            JsonObject jObject = JsonSerializer.Deserialize<JsonObject>(value, JsonSerializerHold.jsonSerializerOptions)!;
            deserialize(jObject, JsonSerializerHold.jsonSerializerOptions);
        }
    }

    public StructReadLockContext lockForRead() {
        return new StructReadLockContext(rwLock);
    }

    public StructWriteLockContext lockForWrite() {
        return new StructWriteLockContext(rwLock);
    }

    protected partial void onBeforeSerialize(JsonSerializerOptions jsonSerializerOptions) { }
    protected partial void onAfterSerialize(JsonObject jObject, JsonSerializerOptions jsonSerializerOptions) { }
    protected partial void onBeforeDeserialize(JsonObject data, JsonSerializerOptions jsonSerializerOptions) { }
    protected partial void onAfterDeserialize(JsonObject data, JsonSerializerOptions jsonSerializerOptions) { }

}
