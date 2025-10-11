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

    public virtual ResourceLocation name { get => _name; init => _name = value; }

    public RegisterManage registerManage { get; internal set; } = null !;

    public int priority { get; init; }

    public int index { get; internal set; }

    public RegisterSystem registerSystem { get; internal set; } = null !;

    public RegisterBasics? basics { get; internal set; }

    protected bool isSonRegister => basics is not null;

    public int globalId { get; } = staticGlobalId++;

    public IReadOnlyList<RegisterBasics> sonRegisterList { get; internal set; } = null!;

    public RegisterBasics() {
    }

    /// <summary>
    /// 最早的初始化方法
    /// </summary>
    public virtual void awakeInit() {

    }

    public virtual void setup() { }

    /// <summary>
    /// 获取附加的注册项目
    /// </summary>
    public virtual IEnumerable<(RegisterBasics son, string name)> getAdditionalRegister() {
        /*return sonRegisterList = GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .OfType<MemberInfo>()
            .Concat(GetType().GetProperties(BindingFlags.Public | BindingFlags.Static))
            .Where(Util.isEffective)
            .Select(memberInfo => (memberInfo, type: ((memberInfo as FieldInfo)?.FieldType ?? (memberInfo as PropertyInfo)?.PropertyType)!, attribute: memberInfo.GetCustomAttribute<FieldRegisterAttribute>()))
            .Where(t => t.attribute is not null)
            .Where(t => t.type is not null)
            .Diversion(
                t => t.attribute.controlRegistration is not null,
                lt => lt
                    .Where(
                        t =>
                            (GetType().GetProperty(t.attribute.controlRegistration!, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(this) as bool? ?? false) ||
                            (GetType().GetField(t.attribute.controlRegistration!, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(this) as bool? ?? false)
                    )
                    .Exception(e => registerSystem.log?.error($"获取注册控制属性值时发生异常: {e.Message}", e))
                    .NotNull()
            )
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
                    .Exception(e => registerSystem.log?.error($"创建 RegisterBasics 实例时发生异常，类型: {GetType().Name}", e))
                    .NotNull()
                    .Peek(
                        t => {
                            (t.memberInfo as PropertyInfo)?.SetValue(null, t.registerBasics);
                            (t.memberInfo as FieldInfo)?.SetValue(null, t.registerBasics);
                        }
                    )
                    .Exception(e => registerSystem.log?.error($"设置静态属性或字段值时发生异常，成员: {GetType().Name}", e))
                    .NotNull()
            )
            .Peek(t => t.registerBasics.name ??= new ResourceLocation(name.domain, $"{name.path}.{t.memberInfo.Name}"))
            .Diversion(
                t => t.registerBasics.registerManage is null,
                lt => lt
                    .Peek(t => t.registerBasics.registerManage = registerSystem.getRegisterManageOfRegisterType(t.registerBasics.GetType())!)
                    .Where(t => t.registerBasics.registerManage is not null, t => registerSystem.log?.error($"未找到 RegisterBasics 对应的 RegisterManage，类型: {t.registerBasics.GetType().Name}"))
            )
            .Peek(t => t.registerBasics.registerSystem = registerSystem)
            .Peek(t => t.registerBasics.basics = this)
            .Select(t => t.registerBasics)
            .ToList();*/

        yield break;
    }

    /// <summary>
    /// 初始化方法
    /// </summary>
    public virtual void init() {
    }

    /// <summary>
    /// 初始化结束后统一调用
    /// </summary>
    public virtual void initEnd() {
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
