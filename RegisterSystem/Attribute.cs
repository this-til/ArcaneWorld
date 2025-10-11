using System;

namespace RegisterSystem;

/// <summary>
/// 忽略注册字段
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class IgnoreRegisterAttribute : Attribute {

}