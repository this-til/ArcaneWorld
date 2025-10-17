using System.Runtime.CompilerServices;


[AttributeUsage(AttributeTargets.Class, Inherited = false)]
class ClassNameAttribute : System.Attribute {
    // [DateTimeConstantAttribute(1675150868847)]
    // public DateTime timestamp;
    public string Name = "";
    public string Icon = "";
    public string ScriptPath = "";

    public ClassNameAttribute([CallerFilePath] string scriptPath = "") {
        ScriptPath = scriptPath;
    }
}