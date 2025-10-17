namespace CakeToolset.Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class JsonConverterAutomaticLoadAttribute: System.Attribute {

    public int priority { get; }

    public JsonConverterAutomaticLoadAttribute(int priority = 0) => this.priority = priority;

}
