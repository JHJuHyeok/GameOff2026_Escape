using UnityEngine;

namespace UtilityTool
{
    public static class ColorExtensions
    {
        public static Color WithAlpha(this Color c, float alpha) => new Color(c.r, c.g, c.b, alpha);

        /// <summary>"RRGGBB" (# 없음)</summary>
        public static string ToHex(this Color c) => ColorUtility.ToHtmlStringRGB(c);

        /// <summary>TMP 리치 텍스트로 감싸기: "text".Colorize(Color.red)</summary>
        public static string Colorize(this string text, Color c) => $"<color=#{c.ToHex()}>{text}</color>";
    }
}
