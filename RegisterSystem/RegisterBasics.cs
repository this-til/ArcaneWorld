using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonUtil.Extensions;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace RegisterSystem;

public class RegisterBasics : IComparable<RegisterBasics> {

    protected static int staticGlobalId;

    internal ResourceLocation _name = null !;

    public ResourceLocation name { get => _name; init => _name = value; }

    public RegisterManage registerManage { get; internal set; } = null !;

    public int priority { get; init; }

    public int index { get; internal set; }

    public RegisterSystem registerSystem { get; internal set; } = null !;

    public RegisterBasics? basics { get; internal set; }

    protected bool isSonRegister => basics is not null;

    public int globalId { get; } = staticGlobalId++;

    public IReadOnlyList<RegisterBasics> sonRegisterList { get; internal set; } = null!;

    protected internal RegisterBasics() {
    }

    /// <summary>
    /// 最早的初始化方法
    /// </summary>
    protected internal virtual void awakeInit() {

    }

    protected internal virtual void setup() { }

    /// <summary>
    /// 获取附加的注册项目
    /// </summary>
    protected internal virtual IEnumerable<(RegisterBasics son, string name)> getAdditionalRegister() {
        yield break;
    }

    /// <summary>
    /// 初始化方法
    /// </summary>
    protected internal virtual void init() {
    }

    /// <summary>
    /// 初始化结束后统一调用
    /// </summary>
    protected internal virtual void initEnd() {
    }
    
    protected internal virtual void dispose() {
        
    }

    public override string ToString() => name?.ToString() ?? "(not specified)";

    public override int GetHashCode() => globalId;

    public int CompareTo(RegisterBasics other) {
        int _priority = -priority.CompareTo(other.priority);
        if (_priority == 0) {
            _priority = globalId.CompareTo(other.globalId);
        }
        return _priority;
    }

}
