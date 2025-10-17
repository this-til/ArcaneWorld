using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonUtil.Extensions;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace RegisterSystem;

public abstract class RegisterManage : IComparable<RegisterManage> {

    protected static int staticGlobalId;

    public ResourceLocation? _name;

    /// <summary>
    /// 获取或设置名称，不能为空
    /// </summary>
    public ResourceLocation name => _name ??= new ResourceLocation(GetType().Namespace ?? "null", GetType().Name);

    /// <summary>
    /// 获取或设置全局ID，只读
    /// </summary>
    public int globalId { get; } = staticGlobalId++;

    /// <summary>
    /// 优先级
    /// </summary>
    public virtual int priority { get; } = 0;

    public RegisterSystem registerSystem { get; internal set; } = null !;

    /// <summary>
    /// 作为基础的类管理类型
    /// </summary>
    public RegisterManage? basicsRegisterManage { get; internal set; } = null !;

    public RegisterManage rootRegisterManage => basicsRegisterManage is null
        ? this
        : basicsRegisterManage.rootRegisterManage;

    /// <summary>
    /// 所有的子RegisterManage
    /// </summary>
    public IReadOnlyList<RegisterManage> sonRegisterManage { get; internal set; } = null!;

    /// <summary>
    /// 直系RegisterManage
    /// </summary>
    public IReadOnlyList<RegisterManage> directSonRegisterManage { get; internal set; } = null!;

    public abstract IEnumerable<ResourceLocation> keys { get; }

    public abstract IReadOnlyList<RegisterBasics> valueErases { get; }

    public abstract int count { get; }

    /// <summary>
    /// 获取当前管理的类型
    /// </summary>
    public abstract Type registerType { get; }

    /// <summary>
    /// 获取上一级的管理类的类型
    /// </summary>
    public virtual Type? basicsRegisterManageType => getBasicsRegisterManageType();

    public virtual Type? getBasicsRegisterManageType() => null;

    public abstract RegisterBasics getErase(ResourceLocation key);
    public abstract RegisterBasics indexErase(int i);
    public abstract bool contain(RegisterBasics registerBasics);

    protected internal virtual void awakeInit() {
    }

    protected internal virtual void setup() { }

    protected internal virtual IEnumerable<(RegisterBasics registerBasics, string name)> getDefaultRegisterItem() {
        yield break;
    }

    protected internal virtual void init() {
    }

    protected internal virtual IEnumerable<(RegisterBasics registerBasics, string name)> getSecondDefaultRegisterItem() {
        yield break;
    }

    protected internal virtual void initSecond() {
    }

    protected internal virtual void initThird() {
    }

    protected internal virtual void initEnd() {
    }

    protected internal virtual void dispose() {
        
    }

    /// <summary>
    /// 注册操作
    /// </summary>
    protected internal abstract void put(RegisterBasics register, bool fromSon);

    /// <summary>
    /// 获取哈希码
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => globalId;

    public int CompareTo(RegisterManage other) {
        int _priority = -priority.CompareTo(other.priority);
        if (_priority == 0) {
            _priority = globalId.CompareTo(other.globalId);
        }

        return _priority;
    }

}

public abstract class RegisterManage<T> : RegisterManage where T : RegisterBasics {

    protected List<T> registerBasicsList = new List<T>();

    protected Dictionary<ResourceLocation, T> registerBasicsMap = new Dictionary<ResourceLocation, T>();

    public override bool contain(RegisterBasics registerBasics) => registerBasics is T t && registerBasicsMap.ContainsValue(t);

    public override IEnumerable<ResourceLocation> keys { get => registerBasicsMap.Keys; }

    public override IReadOnlyList<RegisterBasics> valueErases { get => registerBasicsList; }

    public virtual IReadOnlyList<T> values { get => registerBasicsList; }

    public override int count { get => registerBasicsList.Count; }

    public override Type registerType { get => typeof(T); }

    public override RegisterBasics getErase(ResourceLocation key) => get(key);

    public override RegisterBasics indexErase(int i) => registerBasicsList[i];

    public T get(ResourceLocation key) {
        if (registerBasicsMap.TryGetValue(key, out var a)) {
            return a;
        }
        return null!;
    }

    public T index(int i) {
        if (i < 0 || i >= registerBasicsList.Count) {
            return null!;
        }
        return registerBasicsList[i];
    }

    protected internal override void put(RegisterBasics register, bool fromSon) {
        basicsRegisterManage?.put(register, true);
        for (var i = 0; i < registerBasicsList.Count + 1; i++) {
            if (i == registerBasicsList.Count) {
                registerBasicsList.Add((T)register);
                break;
            }

            if (register.CompareTo(registerBasicsList[i]) < 0) {
                registerBasicsList.Insert(i, (T)register);
                break;
            }
        }

        registerBasicsMap.Add(register.name, (T)register);
    }

    protected internal override void initThird() {
        base.initThird();
        if (basicsRegisterManage is not null) {
            return;
        }

        for (var i = 0; i < registerBasicsList.Count; i++) {
            registerBasicsList[i].index = i;
        }
    }

}