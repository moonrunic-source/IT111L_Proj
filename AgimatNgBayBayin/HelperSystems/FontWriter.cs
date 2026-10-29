using System.Collections.Generic;
using System.Drawing;

namespace AgimatNgBayBayin.HelperSystems
{
    /// <summary>Creates each font once and reuses it (drawing runs 60 times per second).</summary>
    public static class FontWriter
    {
        private static readonly Dictionary<string, Font> Cache = new Dictionary<string, Font>();
        private static readonly StringFormat CenterFormat = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.NoWrap
        };
        public static Font Get(string fontName, float size, FontStyle style = FontStyle.Regular)
        {
            string key = fontName + "|" + size + "|" + (int)style;

            Font font;
            if (!Cache.TryGetValue(key, out font))
            {
                font = new Font(fontName, size, style, GraphicsUnit.Point);
                Cache[key] = font;
            }
            return font;
        }
        public static void DrawCentered(Graphics g, string text, Font font, Brush brush, RectangleF area)
        {
            g.DrawString(text, font, brush, area, CenterFormat);
        }

    }
}
