using System;
using System.Reflection;

namespace RegisterSystem;

/// <summary>
/// 工具类
/// </summary>
public class Util {

    /// <summary>
    /// 判断类型是否有效
    /// </summary>
    public static bool isEffective(Type type) {
        if (type.IsAbstract) {
            return false;
        }
        if (type.GetCustomAttribute<ObsoleteAttribute>() is not null) {
            return false;
        }
        if (type.GetCustomAttribute<IgnoreRegisterAttribute>() is not null) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 判断成员信息是否有效
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <returns></returns>
    public static bool isEffective(MemberInfo memberInfo) {
        if (memberInfo.GetCustomAttribute<ObsoleteAttribute>() is not null) {
            return false;
        }
        if (memberInfo.GetCustomAttribute<IgnoreRegisterAttribute>() is not null) {
            return false;
        }
        return true;
    }

}