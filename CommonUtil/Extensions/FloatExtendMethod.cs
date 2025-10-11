namespace CommonUtil.Extensions;

public static class FloatExtendMethod {

    public static float NotNan(this float s, float def = 0) => float.IsNaN(s)
        ? def
        : s;

}
