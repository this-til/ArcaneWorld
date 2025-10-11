using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace RegisterSystem;

public abstract class RegisterSystemException : Exception {

    public RegisterSystem registerSystem { get; }

    protected RegisterSystemException(RegisterSystem registerSystem) {
        this.registerSystem = registerSystem;
    }

    protected RegisterSystemException(SerializationInfo info, StreamingContext context, RegisterSystem registerSystem) : base(info, context) {
        this.registerSystem = registerSystem;
    }

    protected RegisterSystemException(string message, RegisterSystem registerSystem) : base(message) {
        this.registerSystem = registerSystem;
    }

    protected RegisterSystemException(string message, Exception innerException, RegisterSystem registerSystem) : base(message, innerException) {
        this.registerSystem = registerSystem;
    }

}

public class RegisterManageRootTypeRepeatedDefinitionException : RegisterSystemException {

    public Type type { get; }

    public List<RegisterManage> registerManages { get; }

    public RegisterManageRootTypeRepeatedDefinitionException(RegisterSystem registerSystem, Type type, List<RegisterManage> registerManages) : base(
        $"注册管理器根类型重复定义：类型 {type.FullName} 被多个注册管理器重复定义",
        registerSystem
    ) {
        this.type = type;
        this.registerManages = registerManages;
    }

}

public class RegisterManageMissingBaseException : RegisterSystemException {

    public RegisterManage registerManage { get; }

    public RegisterManageMissingBaseException(RegisterSystem registerSystem, RegisterManage registerManage) : base(
        $"注册管理器缺少基础：注册管理器 {registerManage.name} 缺少必要的基础管理器",
        registerSystem
    ) {
        this.registerManage = registerManage;
    }

}
    
public class RegisterManageMismatchBaseException  : RegisterSystemException {

    public RegisterManage baseRegisterManage { get; }
    public RegisterManage registerManage { get; }

    public RegisterManageMismatchBaseException(RegisterSystem registerSystem, RegisterManage baseRegisterManage, RegisterManage registerManage) : base(
        $"注册管理器基础不匹配：注册管理器 {registerManage.name} 的基础管理器 {baseRegisterManage.name} 不匹配",
        registerSystem
    ) {
        this.baseRegisterManage = baseRegisterManage;
        this.registerManage = registerManage;
    }

}

/*
public class RegisterManageTypeConflictException : RegisterSystemException {

    public RegisterManage newRegisterManage { get; }

    public RegisterManage oldRegisterManage { get; }

    public Type type { get; }

    public RegisterManageTypeConflictException(RegisterSystem registerSystem, RegisterManage newRegisterManage, RegisterManage oldRegisterManage, Type type) : base(
        $"注册管理器类型冲突：类型 {type.FullName} 已被 {oldRegisterManage.name} 管理，无法被 {newRegisterManage.name} 重复管理",
        registerSystem
    ) {
        this.newRegisterManage = newRegisterManage;
        this.oldRegisterManage = oldRegisterManage;
        this.type = type;
    }

}

public class RegisterManageBasicsTypeMismatchException : RegisterSystemException {

    public RegisterManage registerManage { get; }

    public RegisterManageBasicsTypeMismatchException(RegisterSystem registerSystem, RegisterManage registerManage) : base(
        $"注册管理器基础类型不匹配：{registerManage.name} 的基础管理器类型与其管理的注册类型不兼容",
        registerSystem
    ) {
        this.registerManage = registerManage;
    }

}*/