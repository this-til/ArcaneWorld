﻿using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fractural.Tasks.Internal
{
    internal static class DiagnosticsExtensions
    {
        static bool displayFilenames = true;

        static readonly Regex typeBeautifyRegex = new Regex("`.+$", RegexOptions.Compiled);

        static readonly Dictionary<Type, string> builtInTypeNames = new Dictionary<Type, string>
        {
            { typeof(void), "void" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" },
            { typeof(Task), "Task" },
            { typeof(GDTask), "GDTask" },
            { typeof(GDTaskVoid), "GDTaskVoid" }
        };

		[RequiresUnreferencedCode("Calls System.Diagnostics.StackFrame.GetMethod()")]
		public static string CleanupAsyncStackTrace(this StackTrace stackTrace)
        {
            if (stackTrace == null) {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                StackFrame sf = stackTrace.GetFrame(i);

                MethodBase mb = sf.GetMethod();

                if (IgnoreLine(mb)) {
                    continue;
                }
                if (IsAsync(mb))
                {
                    sb.Append("async ");
                    TryResolveStateMachineMethod(ref mb, out Type decType);
                }

                // return type
                if (mb is MethodInfo mi)
                {
                    sb.Append(BeautifyType(mi.ReturnType, false));
                    sb.Append(" ");
                }

                // method name
                sb.Append(BeautifyType(mb.DeclaringType, false));
                if (!mb.IsConstructor)
                {
                    sb.Append(".");
                }
                sb.Append(mb.Name);
                if (mb.IsGenericMethod)
                {
                    sb.Append("<");
                    foreach (Type item in mb.GetGenericArguments())
                    {
                        sb.Append(BeautifyType(item, true));
                    }
                    sb.Append(">");
                }

                // parameter
                sb.Append("(");
                sb.Append(string.Join(", ", mb.GetParameters().Select(p => BeautifyType(p.ParameterType, true) + " " + p.Name)));
                sb.Append(")");

                // file name
                if (displayFilenames && (sf.GetILOffset() != -1))
                {
                    String fileName = null;

                    try
                    {
                        fileName = sf.GetFileName();
                    }
                    catch (NotSupportedException)
                    {
                        displayFilenames = false;
                    }
                    catch (SecurityException)
                    {
                        displayFilenames = false;
                    }

                    if (fileName != null)
                    {
                        sb.Append(' ');
                        sb.AppendFormat(CultureInfo.InvariantCulture, "(at {0})", AppendHyperLink(fileName, sf.GetFileLineNumber().ToString()));
                    }
                }

                sb.AppendLine();
            }
            return sb.ToString();
        }

        static bool IsAsync(MethodBase methodInfo)
        {
            Type declareType = methodInfo.DeclaringType;
            return typeof(IAsyncStateMachine).IsAssignableFrom(declareType);
        }

		// code from Ben.Demystifier/EnhancedStackTrace.Frame.cs
		[RequiresUnreferencedCode("Calls System.Type.GetMethods()")]
		static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
        {
            declaringType = method.DeclaringType;

            Type parentType = declaringType.DeclaringType;
            if (parentType == null)
            {
                return false;
            }

            MethodInfo[] methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methods == null)
            {
                return false;
            }

            foreach (MethodInfo candidateMethod in methods)
            {
                IEnumerable<StateMachineAttribute> attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>(false);
                if (attributes == null)
                {
                    continue;
                }

                foreach (StateMachineAttribute asma in attributes)
                {
                    if (asma.StateMachineType == declaringType)
                    {
                        method = candidateMethod;
                        declaringType = candidateMethod.DeclaringType;
                        // Mark the iterator as changed; so it gets the + annotation of the original method
                        // async statemachines resolve directly to their builder methods so aren't marked as changed
                        return asma is IteratorStateMachineAttribute;
                    }
                }
            }

            return false;
        }

        static string BeautifyType(Type t, bool shortName)
        {
            if (builtInTypeNames.TryGetValue(t, out var builtin))
            {
                return builtin;
            }
            if (t.IsGenericParameter) {
                return t.Name;
            }
            if (t.IsArray) {
                return BeautifyType(t.GetElementType(), shortName) + "[]";
            }
            if (t.FullName?.StartsWith("System.ValueTuple") ?? false)
            {
                return "(" + string.Join(", ", t.GetGenericArguments().Select(x => BeautifyType(x, true))) + ")";
            }
            if (!t.IsGenericType) {
                return shortName ? t.Name : t.FullName.Replace("GDTask.Triggers.", "").Replace("GDTask.Internal.", "").Replace("GDTask.", "") ?? t.Name;
            }

            var innerFormat = string.Join(", ", t.GetGenericArguments().Select(x => BeautifyType(x, true)));

            var genericType = t.GetGenericTypeDefinition().FullName;
            if (genericType == "System.Threading.Tasks.Task`1")
            {
                genericType = "Task";
            }

            return typeBeautifyRegex.Replace(genericType, "").Replace("GDTask.Triggers.", "").Replace("GDTask.Internal.", "").Replace("GDTask.", "") + "<" + innerFormat + ">";
        }

        static bool IgnoreLine(MethodBase methodInfo)
        {
            var declareType = methodInfo.DeclaringType.FullName;
            if (declareType == "System.Threading.ExecutionContext")
            {
                return true;
            }
            else if (declareType.StartsWith("System.Runtime.CompilerServices"))
            {
                return true;
            }
            else if (declareType.StartsWith("GDTask.CompilerServices"))
            {
                return true;
            }
            else if (declareType == "System.Threading.Tasks.AwaitTaskContinuation")
            {
                return true;
            }
            else if (declareType.StartsWith("System.Threading.Tasks.Task"))
            {
                return true;
            }
            else if (declareType.StartsWith("GDTask.GDTaskCompletionSourceCore"))
            {
                return true;
            }
            else if (declareType.StartsWith("GDTask.AwaiterActions"))
            {
                return true;
            }

            return false;
        }

        static string AppendHyperLink(string path, string line)
        {
            FileInfo fi = new FileInfo(path);
            if (fi.Directory == null)
            {
                return fi.Name;
            }
            else
            {
                var fname = fi.FullName.Replace(System.IO.Path.DirectorySeparatorChar, '/').Replace(ProjectSettings.GlobalizePath("res://"), "");
                var withAssetsPath = "Assets/" + fname;
                return "<a href=\"" + withAssetsPath + "\" line=\"" + line + "\">" + withAssetsPath + ":" + line + "</a>";
            }
        }
    }
}

