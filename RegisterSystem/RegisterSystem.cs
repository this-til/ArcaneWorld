using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonUtil.Extensions;
using log4net;

namespace RegisterSystem;

public partial class RegisterSystem {

    /// <summary>
    /// 所有接受管理的程序集
    /// </summary>
    public required HashSet<Assembly> managedAssemblySet { get; init; }

    /// <summary>
    /// 允许检测的所有类型
    /// </summary>
    public IReadOnlyList<Type> allType { get; private set; } = null!;

    #region registerManage

    public IReadOnlyDictionary<Assembly, IReadOnlyList<Type>> managedRegisterManageTypeMap { get; private set; } = null!;

    public IReadOnlyDictionary<Assembly, IReadOnlyList<RegisterManage>> managedRegisterManageMap { get; private set; } = null!;

    public IReadOnlyList<Type> manageTypeList { get; private set; } = null!;

    public IReadOnlyList<RegisterManage> manageList { get; private set; } = null!;

    /// <summary>
    /// 根据<see cref = "RegisterManage.getName()"/>类型的映射表
    /// </summary>
    public IReadOnlyDictionary<ResourceLocation, RegisterManage> nameManageMap { get; private set; } = null!;

    /// <summary>
    /// 根据<see cref = "typeof(RegisterManage)"/>类型的映射表
    /// </summary>
    public IReadOnlyDictionary<Type, RegisterManage> manageMap { get; private set; } = null!;

    public IReadOnlyList<RegisterManage> rootManageList { get; private set; } = null!;

    public IReadOnlyDictionary<Type, RegisterManage> rootManageMap { get; private set; } = null!;

    /// <summary>
    /// 依据注册项目的类型
    /// </summary>
    public IReadOnlyDictionary<Type, RegisterManage> rootRegisterTypeManageMap { get; private set; } = null!;

    #endregion

    #region registerBasics

    public IReadOnlyCollection<RegisterBasics> registerBasicsSortedSet => _registerBasicsSortedSet;

    protected readonly SortedSet<RegisterBasics> _registerBasicsSortedSet = new SortedSet<RegisterBasics>();

    #endregion

    public LogLevel logLevel { get; init; } = LogLevel.DEBUG;

    private ILog? _rawLog;

    public ILog? log {
        get => _rawLog;
        init => _rawLog = value;
    }

    public void initRegisterSystem() {

        allType = managedAssemblySet.SelectMany(a => a.GetTypes())
            .Where(Util.isEffective)
            .ToList();

        manageTypeList = allType
            .Where(typeof(RegisterManage).IsAssignableFrom)
            .Where(t => !t.IsAbstract)
            .Where(t => t.BaseType is not null)
            .Where(
                t => t.BaseType!.IsAbstract,
                t => log?.Warn($"{t} 其基类 {t.BaseType} 必须是抽象的，系统将忽视它")
            )
            .ToList();

        managedRegisterManageTypeMap = manageTypeList.ClassifiedCollection(
                r => r.Assembly,
                r => r,
                a => new List<Type>(),
                managedAssemblySet
            )
            .ToDictionary(e => e.Key, IReadOnlyList<Type> (e) => e.Value.AsReadOnly())
            .ToReadOnlyDictionary();

        manageList = manageTypeList
            .TrySelect(
                t => (RegisterManage)Activator.CreateInstance(t),
                (t, e) => log?.Error("创建 {t} 实例时出现异常", e)
            )
            .NotNull()
            .Peek(m => m.registerSystem = this)
            .OrderByDescending(r => r.priority)
            .ToList();

        nameManageMap = manageList.ToDictionary(m => m.name, m => m);

        manageMap = manageList.ToDictionary(m => m.GetType(), m => m);

        managedRegisterManageMap = managedRegisterManageTypeMap
            .ToDictionary(
                e => e.Key,
                e => e.Value.Select(t => manageMap[t]).AsReadOnly()
            )
            .ToReadOnlyDictionary();

        rootManageList = manageList
            .Where(m => m.basicsRegisterManageType is null)
            .OrderByDescending(m => m.priority)
            .ToList();

        rootManageMap = rootManageList.ToDictionary(m => m.GetType(), m => m);

        rootRegisterTypeManageMap = rootManageList
            .Select(m => (m.registerType, m))
            .GroupBy(t => t.registerType)
            .Where(
                r => r.Count() == 1,
                g => throw new RegisterManageRootTypeRepeatedDefinitionException(
                    this,
                    g.Key,
                    g.Select(t => t.m).ToList()
                )
            )
            .ToDictionary(g => g.Key, g => g.First().m);

        // 类型检查
        manageList
            .Select(m => (baseType: m.basicsRegisterManageType, m))
            .Where(t => t.baseType is not null)
            .Select(
                t => (
                    baseManag: manageMap!.GetValueOrDefault(t.baseType),
                    t.m
                )
            )
            .Where(
                t => t.baseManag is not null,
                t => throw new RegisterManageMissingBaseException(this, t.m)
            )
            .Where(
                t => t.baseManag!.registerType.IsAssignableFrom(t.m.registerType),
                t => throw new RegisterManageMismatchBaseException(this, t.baseManag!, t.m)
            )
            .Peek(t => t.m.basicsRegisterManage = t.baseManag)
            .End();

        manageList
            .Select(
                m => (
                    m,
                    sonList: manageList
                        .Where(mm => Equals(mm.basicsRegisterManage, m))
                        .ToList()
                        .AsReadOnly()
                )
            )
            .Peek(
                t => t.m.sonRegisterManage = t.sonList
            )
            .End();

        void collectDirectSon(RegisterManage registerManage, List<RegisterManage> list) {
            list.AddRange(registerManage.sonRegisterManage);
            foreach (RegisterManage manage in registerManage.sonRegisterManage) {
                collectDirectSon(manage, list);
            }
        }

        manageList
            .Select(
                m => {
                    List<RegisterManage> directSonList = new List<RegisterManage>();
                    collectDirectSon(m, directSonList);
                    return (m, directSonList: directSonList.AsReadOnly());
                }
            )
            .Peek(t => t.m.directSonRegisterManage = t.directSonList)
            .End();

        manageList
            .TryPeek(
                m => m.awakeInit(),
                (m, e) => log?.Error($"调用 {m.GetType()}.awakeInit() 时出现异常：", e)
            )
            .End();

        manageList
            .TryPeek(
                m => m.setup(),
                (m, e) => log?.Error($"调用 {m.GetType()}.setup() 时出现异常：", e)
            )
            .End();

        List<RegisterBasics> registerItemList = manageList
            .TrySelectMany(
                m => m.getDefaultRegisterItem()
                    .Select(t => (registerManage: m, t.registerBasics, t.name)),
                (m, e) => log?.Error($"调用 {m.GetType()}.getDefaultRegisterItem() 时出现异常：", e)
            )
            .Peek(t => t.registerBasics._name = new ResourceLocation(t.registerManage.name.domain, t.name))
            .Peek(t => t.registerBasics.registerManage = t.registerManage)
            .Peek(t => t.registerBasics.registerSystem = this)
            .Select(t => t.registerBasics)
            .NotNull()
            .Distinct()
            .ToList();

        unifyRegister(registerItemList);

        manageList
            .TryPeek(
                m => m.init(),
                (m, e) => log?.Error($"调用 {m.GetType()}.init() 时出现异常：", e)
            )
            .End();

        registerItemList = manageList
            .TrySelectMany(
                m => m.getSecondDefaultRegisterItem()
                    .Select(t => (registerManage: m, t.registerBasics, t.name)),
                (m, e) => log?.Error($"调用 {m.GetType()}.getSecondDefaultRegisterItem() 时出现异常：", e)
            )
            .Peek(t => t.registerBasics._name = new ResourceLocation(t.registerManage.name.domain, t.name))
            .Peek(t => t.registerBasics.registerManage = t.registerManage)
            .Peek(t => t.registerBasics.registerSystem = this)
            .Select(t => t.registerBasics)
            .NotNull()
            .Distinct()
            .ToList();

        unifyRegister(registerItemList);

        manageList
            .TryPeek(
                m => m.initSecond(),
                (m, e) => log?.Error($"调用 {m.GetType()}.initSecond() 时出现异常：", e)
            )
            .End();

        manageList
            .TryPeek(
                m => m.initThird(),
                (m, e) => log?.Error($"调用 {m.GetType()}.initThird() 时出现异常：", e)
            )
            .End();

        _registerBasicsSortedSet
            .TryPeek(
                r => r.initEnd(),
                (r, e) => log?.Error($"调用 ({r.GetType()}-{r.name}).initEnd() 时出现异常：", e)
            )
            .End();

        manageList
            .TryPeek(
                m => m.initEnd(),
                (m, e) => log?.Error($"调用 {m.GetType()}.initEnd() 时出现异常：", e)
            )
            .End();
    }

    protected void unifyRegister(List<RegisterBasics> registerBasicsList) {
        registerBasicsList = registerBasicsList
            .Where(
                r => r.registerManage is not null,
                r => log?.Warn($"注册项：{r.GetType()}-{r.name} 缺少RegisterManage，系统将忽略它")
            )
            .Peek(r => r.registerSystem = this)
            .TryPeek(
                r => r.awakeInit(),
                (r, e) => log?.Error($"调用 ({r.GetType()}-{r.name}).awakeInit() 时出现异常：", e)
            )
            .TryPeek(
                r => r.setup(),
                (r, e) => log?.Error($"调用 ({r.GetType()}-{r.name}).setup() 时出现异常：", e)
            )
            .Peek(r => r.registerManage.put(r, false))
            .Peek(r => _registerBasicsSortedSet.Add(r))
            .ToList();

        List<RegisterBasics> additionalList = registerBasicsList
            .Select(r => (basics: r, sonList: r.getAdditionalRegister().ToList()))
            .Peek(
                t =>
                    t.basics.sonRegisterList = t.sonList
                        .Select(st => st.son)
                        .AsReadOnly()
            )
            .SelectMany(t => t.sonList.Select(st => (t.basics, st.son, st.name)))
            .Peek(t => t.son._name ??= new ResourceLocation(t.basics.name.domain, $"{t.basics.name.path}/{t.name}"))
            .Peek(t => t.son.basics = t.basics)
            .Select(t => t.son)
            .Peek(r => r.registerManage ??= getRegisterManageOfRegisterType(r.GetType())!)
            .Where(
                r => r.registerManage is not null,
                r => log?.Warn($"{r.GetType()}-{r.name} 丢失RegisterManage，系统将忽略它")
            )
            .Peek(r => r.registerSystem = this)
            .Distinct()
            .ToList();

        registerBasicsList
            .TryPeek(
                r => r.init(),
                (r, e) => log?.Error($"调用 ({r.GetType()}-{r.name}).init() 时出现异常：", e)
            )
            .End();

        if (additionalList.Count > 0) {
            unifyRegister(additionalList);
        }
    }

    /// <summary>
    ///  获取指定类型的注册管理器
    /// </summary>
    /// <param name = "registerManageClass"></param>
    /// <returns></returns>
    public RegisterManage? getRegisterManageOfManageType(Type? registerManageClass) => registerManageClass is null
        ? null
        : manageMap.GetValueOrDefault(registerManageClass);

    public T? getRegisterManageOfManageType<T>() where T : RegisterManage {
        return getRegisterManageOfManageType(typeof(T)) as T;
    }

    /// <summary>
    /// 获取指定注册类型的注册管理器
    /// </summary>
    public RegisterManage? getRegisterManageOfRegisterType(Type type) {
        Type? basType = type;
        while (basType is not null) {
            if (rootRegisterTypeManageMap.TryGetValue(basType, out RegisterManage? registerType)) {
                return registerType;
            }
            basType = basType.BaseType;
        }
        return null;
    }

    public RegisterManage? getRegisterManageOfRegisterType<T>() => getRegisterManageOfRegisterType(typeof(T));

    /// <summary>
    /// 获取指定名称的注册管理器
    /// </summary>
    public RegisterManage? getRegisterManageOfName(ResourceLocation? name) => name is null
        ? null
        : nameManageMap.GetValueOrDefault(name);

}
