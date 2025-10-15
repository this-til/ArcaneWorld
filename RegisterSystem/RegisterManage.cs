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

    public virtual void awakeInit() {
    }

    public virtual void setup() { }

    public virtual IEnumerable<(RegisterBasics registerBasics, string name)> getDefaultRegisterItem() {
        /*return GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .OfType<MemberInfo>()
            .Concat(GetType().GetProperties(BindingFlags.Public | BindingFlags.Static))
            .Where(Util.isEffective)
            .Select(memberInfo => (memberInfo, type: ((memberInfo as FieldInfo)?.FieldType ?? (memberInfo as PropertyInfo)?.PropertyType)!))
            .Where(t => t.type is not null)
            .Where(t => typeof(RegisterBasics).IsAssignableFrom(t.type))
            .Select(t => (t.memberInfo, t.type, registerBasics: ((t.memberInfo as FieldInfo)?.GetValue(null) as RegisterBasics ?? (t.memberInfo as PropertyInfo)?.GetValue(null) as RegisterBasics)!))
            .Diversion(
                t => t.registerBasics is null,
                lt => lt
                    .Select(
                        t => t with {
                            registerBasics = (RegisterBasics)Activator.CreateInstance(t.type)
                        }
                    )
                    .Exception(e => { if (registerSystem.log?.IsErrorEnabled ?? false) registerSystem.log?.Error($"调用 {GetType()}.getDefaultRegisterItem() 中创建 RegisterBasics 实例时出现异常：", e); })
                    .NotNull()
                    .Peek(
                        t => {
                            (t.memberInfo as PropertyInfo)?.SetValue(null, t.registerBasics);
                            (t.memberInfo as FieldInfo)?.SetValue(null, t.registerBasics);
                        }
                    )
                    .Exception(e => { if (registerSystem.log?.IsErrorEnabled ?? false) registerSystem.log?.Error($"调用 {GetType()}.getDefaultRegisterItem() 中设置静态属性或字段值时出现异常：", e); })
                    .NotNull()
            )
            .Peek(t => t.registerBasics.name ??= new ResourceLocation(name.domain, t.memberInfo.Name))
            .Peek(t => t.registerBasics.registerManage ??= this)
            .Peek(t => t.registerBasics.registerSystem = registerSystem)
            .Select(t => t.registerBasics);*/
        yield break;
    }

    public virtual void init() {
    }

    public virtual IEnumerable<(RegisterBasics registerBasics, string name)> getSecondDefaultRegisterItem() {
        yield break;
    }

    public virtual void initSecond() {
    }

    public virtual void initThird() {
    }

    public virtual void initEnd() {
    }

    /// <summary>
    /// 注册操作
    /// </summary>
    public abstract void put(RegisterBasics register, bool fromSon);

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

    public override void put(RegisterBasics register, bool fromSon) {
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

    public override void initThird() {
        base.initThird();
        if (basicsRegisterManage is not null) {
            return;
        }

        for (var i = 0; i < registerBasicsList.Count; i++) {
            registerBasicsList[i].index = i;
        }
    }

}