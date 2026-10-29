using AgimatNgBayBayin.Configurations;
using AgimatNgBayBayin.HelperSystems;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Policy;

namespace AgimatNgBayBayin.Assets
{
    public class BayBayinCharacters : Container
    {
        private readonly Image? sprite = LoadImage(Sprites.BayBayinContainerSpritePath);
        private readonly bool _showlatinhint;
        public Syllable _syllable { get; private set; }
        public BayBayinCharacters(float x, float y, float speed, Syllable syllable, bool showLatinHint) : base(x, y, speed)
        {
            _syllable = syllable;
            _showlatinhint = showLatinHint;
        }

        public override void DrawPicture(Graphics g)
        {
            var square = new RectangleF(X, Y, Width, Height);
            if (sprite != null)
            {
                g.DrawImage(sprite, X, Y, Width, Height);
            }
            else
            {
                MessageBox.Show(
                    $"Failed to load image: {Sprites.BayBayinContainerSpritePath}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                g.FillRectangle(Brushes.Magenta, X, Y, Width, Height);
            }

            using (var text = new SolidBrush(FontItems.BaybayinFontColor))
            {
                RectangleF glyphArea = _showlatinhint
                    ? new RectangleF(X, Y + 2, Width, Height - 22)
                    : square;
                FontWriter.DrawCentered(g, _syllable.Glyph, FontWriter.Get(FontItems.GlyphFont, 26f), text, glyphArea);

                if (_showlatinhint)
                {
                    var hintarea = new RectangleF(X, Y + 2, Width, Height);
                    FontWriter.DrawCentered(g, _syllable.Latin, FontWriter.Get(FontItems.TextFont, 10f, FontStyle.Bold), text, hintarea);

                }
            }
        }
    }
}
