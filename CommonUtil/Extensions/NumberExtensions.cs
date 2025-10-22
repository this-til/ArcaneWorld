using System;

namespace CommonUtil.Extensions;

public static class NumberExtensions {

    public static int Wrap(this int index, int length) {
        if (length <= 0) {
            throw new ArgumentException("Length must be positive");
        }
        return (index % length + length) % length;
    }

    public static float NotNan(this float s, float def = 0) => float.IsNaN(s)
        ? def
        : s;

}
