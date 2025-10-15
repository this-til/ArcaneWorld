using System;
using System.Collections.Generic;
using System.Text;

namespace CommonUtil.Extensions;

public static class StringExtendMethod {

    /// <summary>
    /// Replaces the format item in a specified String with the text equivalent of the value of a corresponding Object instance in a specified array.
    /// Using key name in the format string to refer a object in the args dictionary with the same key
    /// </summary>
    /// <param name = "format">A composite format string. </param>
    /// <param name = "args">A dictionary containing zero or more objects to format.</param>
    /// <returns>A copy of format in which the format items have been replaced by the String equivalent of the corresponding instances of Object in args.</returns>
    public static void Format(this string format, StringBuilder? stringBuilder, Action<string> structure) {
        if (format == null || structure == null) {
            throw new ArgumentNullException(
                format == null
                    ? "format"
                    : "args"
            );
        }

        bool startBrace = false;
        int startBraceIndex = -1;
        for (int i = 0; i < format.Length; i++) {
            char c = format[i];
            switch (c) {
                case '{':
                    if (startBrace) {
                        if (i - startBraceIndex == 1) {
                            startBrace = false;
                            stringBuilder?.Append('{');
                            break;
                        }
                    }

                    startBrace = true;
                    startBraceIndex = i;
                    break;
                case '}':
                    if (!startBrace) {
                        if (i + 1 < format.Length && format[i] == '}') {
                            stringBuilder?.Append('}');
                            i++;
                            break;
                        }
                    }

                    startBrace = false;
                    string key = format.Substring(startBraceIndex + 1, i - startBraceIndex - 1);
                    structure(key);
                    break;
                default:
                    if (!startBrace) {
                        stringBuilder?.Append(c);
                    }

                    break;
            }
        }
    }

    public static bool IsNullOrEmpty(this string? self) => string.IsNullOrEmpty(self);
    public static bool IsNullOrWhiteSpace(this string? self) => string.IsNullOrWhiteSpace(self);

}
