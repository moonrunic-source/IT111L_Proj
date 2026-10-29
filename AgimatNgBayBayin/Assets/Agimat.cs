using AgimatNgBayBayin.Configurations;

namespace AgimatNgBayBayin.Assets
{
    public class Agimat : Container
    {
        private static readonly Image? sprite = LoadImage(Sprites.AgimatSpritePath);
        public Agimat(float x, float y, float speed) : base(x, y, speed)
        {
        }
        public override void DrawPicture(Graphics g)
        {
            var hitbox = new RectangleF(X, Y, Width, Height);
            if (sprite != null)
            {
                g.DrawImage(sprite, X, Y, Width, Height);
            }
            else
            {
                MessageBox.Show(
                    $"Failed to load image: {Sprites.AgimatSpritePath}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                g.FillRectangle(Brushes.Magenta, X, Y, Width, Height);
            }
        }
    }
}
