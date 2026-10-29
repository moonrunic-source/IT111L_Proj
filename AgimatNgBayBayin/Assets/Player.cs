using AgimatNgBayBayin.Configurations;

namespace AgimatNgBayBayin.Assets
{
    class Player : BaseEntity
    {
        public bool IsMovingUp { get; set; }
        public bool IsMovingDown { get; set; }
        private static readonly Image? sprite = LoadImage(Sprites.PlayerSpritePath);

        public Player()
        {
            Width = Sprites.PlayerSpriteWidth;
            Height = Sprites.PlayerSpriteHeight;
            X = RunningGame.PlayerInitialXPosition;
            Y = (RunningGame.PlayAreaTop + RunningGame.HudTop) / 2f - Height / 2f;
        }

        public override void Update(float time) 
        {
            float direction = 0;
            if (IsMovingUp)
            {
                direction = -1f;
            }
            else if (IsMovingDown)
            {
                direction = 1f;
            }
            Y += direction * RunningGame.PlayerSpeed * time; // Adjust speed as needed
            float minY = RunningGame.PlayAreaTop;
            float maxY = RunningGame.HudTop - Height;
            Y = Math.Max(minY, Math.Min(maxY, Y));
        }
        public override void DrawPicture(Graphics g)
        {   
            if (sprite != null)
            {
                g.DrawImage(sprite, X, Y, Width, Height);
            }
            else {
                MessageBox.Show(
                    $"Failed to load image: {Sprites.PlayerSpritePath}", 
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                g.FillRectangle(Brushes.Magenta, X, Y, Width, Height);
            }
        }
    }
}
