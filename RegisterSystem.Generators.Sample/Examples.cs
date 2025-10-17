
using System;

namespace RegisterSystem.Generators.Sample;

// 基本示例 - 测试正确的类型约束
public partial class ExampleRegisterItemManage : RegisterManage<ExampleRegisterItem> {

    public static bool attachedConditions { get; } = true;
    
    protected  override void setup() {
        base.setup();
        basicItem = new ExampleRegisterItem();
        anotherItem = new ExampleRegisterItem();

        // 测试子类也应该工作
        derivedItem = new DerivedExampleItem();

        if (attachedConditions) {
            attachedConditionsItem = new DerivedExampleItem();
        }
    }

}

public partial class ExampleRegisterItem : RegisterBasics {

    public ExampleRegisterItem() {
    }

}

public partial class DerivedExampleItem : ExampleRegisterItem {

    public DerivedExampleItem() {
    }

}

// 测试已经存在字段的情况
public partial class ExistingFieldsManage : RegisterManage<ExampleRegisterItem> {

    // 这个字段已经存在且格式正确，应该跳过生成
    public static ExampleRegisterItem correctExistingField { get; private set; } = null!;

    // 这个字段格式错误，应该报告错误
    //public static ExampleRegisterItem wrongField { get; set; }

    // 这个是字段而不是属性，应该报告错误
    //public static ExampleRegisterItem fieldInsteadOfProperty;

    protected override void setup() {
        base.setup();
        correctExistingField = new ExampleRegisterItem();
        wrongField = new ExampleRegisterItem();
        fieldInsteadOfProperty = new ExampleRegisterItem();

        // 这个应该正常生成
        newField = new ExampleRegisterItem();
    }

}

// 测试错误类型约束的情况
public partial class WrongTypeManage : RegisterManage<ExampleRegisterItem> {

    protected  override void setup() {
        base.setup();
        // 这个应该成功生成（正确类型）
        correctType = new ExampleRegisterItem();

        // 这个应该报告错误（错误类型，不会生成字段）
        //wrongType = new OtherRegisterItem();
    }

}

public partial class OtherRegisterItem : RegisterBasics {

    public OtherRegisterItem() {
    }

}

// 测试多层继承
public partial class MultiLevelManage : RegisterManage<BaseRegisterItem> {

    protected  override void setup() {
        base.setup();
        baseItem = new BaseRegisterItem();
        middleItem = new MiddleRegisterItem();
        leafItem = new LeafRegisterItem();
    }

}

public partial class BaseRegisterItem : RegisterBasics {

    public BaseRegisterItem() {
    }

}

public partial class MiddleRegisterItem : BaseRegisterItem {

    public MiddleRegisterItem() {
    }

}

public partial class LeafRegisterItem : MiddleRegisterItem {

    public LeafRegisterItem() {
    }

}
